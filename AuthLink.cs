class AuthLink
{
    static readonly string? CallBackUrl = Environment.GetEnvironmentVariable("Callback_Url");
    static readonly string? ConsumerKey = Environment.GetEnvironmentVariable("Trello_API_Key");
    
    public static string CreateLink(long telegramId)
    {
        string url =
            $"https://trello.com/1/authorize?expiration=never&name=TelegramToTrello" +
            $"&scope=read,write" +
            $"&response_type=fragment&key={ConsumerKey}" +
            $"&return_url={Uri.EscapeDataString(CallBackUrl!)+telegramId}" +
            $"&callback_method=fragment";

        return url;
    }
}