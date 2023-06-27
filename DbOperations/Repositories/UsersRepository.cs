using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;

namespace TelegramToTrello;

public class UsersRepository : IUsersRepository
{
    public async Task<RegisteredUser> Get(int id)
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.Users.FindAsync(id);
        }
    }

    public async Task<RegisteredUser> Get(string id)
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.Users.FirstOrDefaultAsync(u => u.TrelloId == id);
        }
    }

    public async Task<IEnumerable<RegisteredUser>> GetAll()
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.Users
                .Include(u => u.Boards)
                .ToListAsync();
        }
    }

    public async Task Add(RegisteredUser entity)
    {
        using (BotDbContext dbContext = new())
        { 
            dbContext.Users.Add(entity);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task Update(RegisteredUser entity)
    {
        using (BotDbContext dbContext = new())
        { 
            dbContext.Users.Update(entity);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task Delete(RegisteredUser entity)
    {
        using (BotDbContext dbContext = new())
        { 
            dbContext.Users.Remove(entity);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<RegisteredUser> GetUserWithBoards(int id)
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.Users
                .Include(ru => ru.Boards)
                .FirstOrDefaultAsync(um => um.TelegramId == id);
        }
    }
}