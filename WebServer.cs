using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot;

namespace TelegramToTrello;

public class WebServer
{
    public async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseUrls(Environment.GetEnvironmentVariable("WebServer"));
        var app = builder.Build();

        app.UseStaticFiles();
        app.MapGet("/", () => "Nothing to see here.");
        app.MapPost("/trello/authcallback", AuthCallback);
        app.Run();
    }
    
    private async Task AuthCallback(HttpContext context)
    {
        if (context.Request.Method == "POST")
        {
            using (var reader = new StreamReader(context.Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                var json = JsonConvert.DeserializeObject<JObject>(body);
                var token = json["token"].ToString();
                var telegramId = json["state"].ToString();
                Console.WriteLine("Token: " + token);
                
                TrelloOperations trelloOperations = new TrelloOperations();
                
                string trelloID = await trelloOperations.GetTrelloUserId(token);

                DbOperations dbOperations = new DbOperations();
                await dbOperations.AddTrelloTokenAndId(token, trelloID, int.Parse(telegramId));
            }
        }
    }
}