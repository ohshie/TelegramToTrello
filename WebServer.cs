using System.Text.Json;
using TelegramToTrello.ToFromTrello;
using TelegramToTrello.UserRegistration;

namespace TelegramToTrello;

public class WebServer
{
    private IConfiguration _configuration;
    public WebServer()
    {
        _configuration = Configuration.CreateConfiguration();
    }
    public async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseUrls(_configuration.GetSection("WebServer")["WebServer"]);
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
            string body;
            using (var reader = new StreamReader(context.Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            var (token, telegramId) = JsonParse(body);
            var trelloId = await ConvertTokenToTrelloId(token);

            await UpdateDbWithNewUserInfo(token, trelloId, int.Parse(telegramId));
        }
    }

    private (string?, string?) JsonParse(string jsonBody)
    {
        JsonDocument json = JsonDocument.Parse(jsonBody);
        string? token = json.RootElement.GetProperty("token").GetString();
        string? telegramId = json.RootElement.GetProperty("state").GetString();

        return (token, telegramId);
    }
    
    private async Task<string> ConvertTokenToTrelloId(string? token)
    {
        TrelloOperations trelloOperations = new();
        string? trelloId = await trelloOperations.GetTrelloUserId(token);

        return trelloId;
    }

    private async Task UpdateDbWithNewUserInfo(string token, string trelloId, int telegramId)
    {
        UserDbOperations dbOperations = new();
        RegisteredUser? user = await dbOperations.AddTrelloTokenAndId(token, trelloId, telegramId);
        if (user != null)
        {
            SyncService syncService = new();
            await syncService.SyncStateToTrello(user);
        }
        
    }
}