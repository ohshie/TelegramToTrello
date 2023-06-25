namespace TelegramToTrello;

public static class Configuration
{
    public static IConfiguration CreateConfiguration()
    {
        var enviroment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        if (enviroment=="debug")
        {
            builder.AddJsonFile($"appsettings.Development.json", optional: true);
        }
        
        IConfiguration configuration = builder.Build();

        return configuration;
    }
}