class AuthLink
{
    public static string CreateLink(long telegramId)
    {
        string callbackUrl = Configuration.CallbackUrl;
        string trelloKey = Configuration.TrelloKey;
        
        string url =
            $"https://trello.com/1/authorize?expiration=never&name=TelegramToTrello" +
            $"&scope=read,write" +
            $"&response_type=fragment&key={trelloKey}" +
            $"&return_url={Uri.EscapeDataString(callbackUrl!)+telegramId}" +
            $"&callback_method=fragment";

        return url;
    }
}