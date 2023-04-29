using System.Text.Json;

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
                JsonDocument json = JsonDocument.Parse(body);
                string token = json.RootElement.GetProperty("token").GetString();
                string telegramId = json.RootElement.GetProperty("state").GetString();
                
                TrelloOperations trelloOperations = new TrelloOperations();
                
                string trelloID = await trelloOperations.GetTrelloUserId(token);

                DbOperations dbOperations = new DbOperations();
                await dbOperations.AddTrelloTokenAndId(token, trelloID, int.Parse(telegramId));
            }
        }
    }
}