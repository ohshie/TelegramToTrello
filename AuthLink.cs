using TelegramToTrello;

class AuthLink
{
    private static IConfiguration _configuration;
    public AuthLink()
    {
        _configuration = Configuration.CreateConfiguration();
    }
    
    public static string CreateLink(long telegramId)
    {
        var callbackUrl = _configuration.GetSection("WebServer")["CallbackUrl"];
        var consumerKey = _configuration.GetSection("TrelloApi")["TrelloApi"];
        
        string url =
            $"https://trello.com/1/authorize?expiration=never&name=TelegramToTrello" +
            $"&scope=read,write" +
            $"&response_type=fragment&key={consumerKey}" +
            $"&return_url={Uri.EscapeDataString(callbackUrl!)+telegramId}" +
            $"&callback_method=fragment";

        return url;
    }
}