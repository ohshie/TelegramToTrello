using Microsoft.EntityFrameworkCore;
using TelegramToTrello;

class Program
{
    static async Task Main(string[] args)
    {
        using BotDbContext dbContext = new BotDbContext();
        {
            await dbContext.Database.EnsureCreatedAsync();
        }
        
        BotClient botClient = new BotClient();
        Task bot = botClient.BotOperations();
        
        WebServer server = new WebServer();
        Task webServer = server.Run(args);

        await Task.WhenAll(bot, webServer);
        
        Console.ReadLine();
        Environment.Exit(1);
    }
}
