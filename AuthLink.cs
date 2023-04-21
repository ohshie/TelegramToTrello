class AuthLink
{
    static string callBackUrl = Environment.GetEnvironmentVariable("Callback_Url");

    static string consumerKey = Environment.GetEnvironmentVariable("Trello_API_Key");
    public static string CreateLink(long telegramId)
    {
        string url =
            $"https://trello.com/1/authorize?expiration=never&name=TelegramToTrello" +
            $"&scope=read,write" +
            $"&response_type=fragment&key={consumerKey}" +
            $"&return_url={Uri.EscapeDataString(callBackUrl)+telegramId}" +
            $"&callback_method=fragment";

        return url;
    }
}