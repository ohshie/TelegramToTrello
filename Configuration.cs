namespace TelegramToTrello;

public static class Configuration
{
    public static string CallbackUrl { get; private set; }
    public static string ConnectionString { get; private set; }
    public static string BotToken { get; private set; }
    public static int NotificationTimer { get; private set; }
    public static int SyncTimer { get; private set; }
    public static string ServerUrl { get; private set; }
    public static string TrelloKey { get; private set; }
    
    private static readonly IConfiguration _configuration;
    static Configuration()
    {
        _configuration = CreateConfiguration();
    }

    public static void InitializeVariables()
    {
        ConnectionString = _configuration.GetConnectionString("Postgres");
        CallbackUrl = _configuration.GetSection("WebServer")["CallbackUrl"];
        BotToken = _configuration.GetSection("BotToken")["BotToken"];
        NotificationTimer = Convert.ToInt32(_configuration.GetSection("Timers")["NotificationTimer"]);
        SyncTimer = Convert.ToInt32(_configuration.GetSection("Timers")["SyncTimer"]);
        ServerUrl = _configuration.GetSection("WebServer")["WebServer"];
        TrelloKey = _configuration.GetSection("TrelloApi")["TrelloKey"];
    }

    private static IConfiguration CreateConfiguration()
    {
        var enviroment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        var builder = new ConfigurationBuilder();
        if (enviroment=="Development")
        {
            builder.AddJsonFile($"appsettings.Development.json", optional: true);
        }
        else
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        }
        
        IConfiguration configuration = builder.Build();

        return configuration;
    }
}