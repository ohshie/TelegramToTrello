using TelegramToTrello.ToFromTrello;

namespace TelegramToTrello;

public class WebServer
{
    private readonly TrelloOperations _trelloOperations;
    private readonly UserDbOperations _userDbOperations;
    private readonly SyncService _syncService;
    private readonly ILogger<WebServer> _logger;
    private readonly IConfiguration _configuration;

    public WebServer(TrelloOperations trelloOperations, UserDbOperations userDbOperations, SyncService syncService,
        ILogger<WebServer> logger, IConfiguration configuration)
    {
        _trelloOperations = trelloOperations;
        _userDbOperations = userDbOperations;
        _syncService = syncService;
        _logger = logger;
        _configuration = configuration;
    }
    public async Task Run(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseUrls(_configuration.GetSection("WebServer").GetValue<string>("WebServer")!);
        
        var app = builder.Build();
        
        app.UseStaticFiles();
        app.MapGet("/", () => "Nothing to see here.");
        app.MapPost("/trello/authcallback", AuthCallback);
        
        _logger.LogWarning("server started on {Url}", _configuration.GetSection("WebServer").GetValue<string>("WebServer")!);
        
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
        string? trelloId = await _trelloOperations.GetTrelloUserId(token);

        return trelloId;
    }

    private async Task UpdateDbWithNewUserInfo(string token, string trelloId, int telegramId)
    {
        User? user = await _userDbOperations.AddTrelloTokenAndId(token, trelloId, telegramId);
        if (user != null)
        {
            await _syncService.SyncStateToTrello(user);
        }
    }
}