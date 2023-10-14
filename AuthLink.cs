namespace TelegramToTrello;

public class AuthLink
{
    private readonly IConfiguration _configuration;

    public AuthLink(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateLink(long telegramId)
    {
        string callbackUrl = _configuration.GetSection("WebServer").GetValue<string>("CallbackUrl")!;
        string trelloKey = _configuration.GetSection("TrelloApi").GetValue<string>("TrelloKey")!;
        
        string url =
            $"https://trello.com/1/authorize?expiration=never&name=TelegramToTrello" +
            $"&scope=read,write" +
            $"&response_type=fragment&key={trelloKey}" +
            $"&return_url={Uri.EscapeDataString(callbackUrl!)+telegramId}" +
            $"&callback_method=fragment";

        return url;
    }
}