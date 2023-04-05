using Microsoft.EntityFrameworkCore;
using TelegramToTrello;

class Program
{
    static async Task Main(string[] args)
    {
        using BotDbContext dbContext = new BotDbContext();
        {
            await dbContext.Database.MigrateAsync();
            //await dbContext.Database.EnsureCreatedAsync();
        }
        
        BotClient botClient = new BotClient();

        await botClient.BotOperations();
    }
}
