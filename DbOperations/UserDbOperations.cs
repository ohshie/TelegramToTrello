using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;

namespace TelegramToTrello;

public class UserDbOperations
{
    public async Task<bool> RegisterNewUser(Message message)
    {
        await using BotDbContext dbContext = new BotDbContext();
        {
            RegisteredUser? existingUser = await dbContext.Users.FindAsync((int)message.From!.Id);

            if (existingUser == null)
            {
                dbContext.Users.Add(new RegisteredUser
                {
                    TelegramId = (int)message.From.Id,
                    TelegramName = message.From.Username,
                });
                
                await dbContext.SaveChangesAsync();
                return true;
            }
        }
        return false;
    }

    public async Task<RegisteredUser?> AddTrelloTokenAndId(string token, string trelloId, int telegramId)
    {
        await using BotDbContext dbContext = new BotDbContext();
        {
            RegisteredUser? existingUser = await dbContext.Users.FindAsync(telegramId);

            if (existingUser != null)
            {
                existingUser.TrelloToken = token;
                existingUser.TrelloId = trelloId;

                await dbContext.SaveChangesAsync();
            }

            return existingUser;
        }
    }
    
    public async Task<RegisteredUser?> RetrieveTrelloUser(int telegramId)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            RegisteredUser? trelloUser = await dbContext.Users
                .Include(ub => ub.Boards)
                .FirstOrDefaultAsync(um => um.TelegramId == telegramId);

            if (trelloUser != null)
            {
                return trelloUser;
            }
        }
        return null;
    } 
    
    public async Task<List<RegisteredUser>> FetchAllUsers()
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.Users
                .Include(u => u.Boards)
                .ToListAsync();
        }
    }
}