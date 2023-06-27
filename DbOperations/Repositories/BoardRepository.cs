using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello;

public class BoardRepository : IRepository<Board>
{
    public async Task<Board> Get(int id)
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.Boards.FindAsync(id);
        }
    }

    public async Task<Board> Get(string id)
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.Boards.FirstOrDefaultAsync(b => b.TrelloBoardId == id);
        }
    }

    public async Task<IEnumerable<Board>> GetAll()
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.Boards.ToListAsync();
        }
    }

    public async Task Add(Board entity)
    {
        using (BotDbContext dbContext = new())
        {
            dbContext.Boards.Add(entity);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task Update(Board entity)
    {
        using (BotDbContext dbContext = new())
        {
            dbContext.Boards.Update(entity);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task Delete(Board entity)
    {
        using (BotDbContext dbContext = new())
        {
            dbContext.Boards.Remove(entity);
            await dbContext.SaveChangesAsync();
        }
    }
}