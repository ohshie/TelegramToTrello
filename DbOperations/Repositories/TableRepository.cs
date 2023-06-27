using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello;

public class TableRepository : ITableRepository
{
    public async Task<Table> Get(int id)
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.BoardTables.FindAsync(id);
        }
    }
    
    public async Task<Table> Get(string id)
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.BoardTables.FirstOrDefaultAsync(b => b.TableId == id);
        }
    }

    public async Task<IEnumerable<Table>> GetAll()
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.BoardTables.ToListAsync();
        }
    }

    public async Task Add(Table entity)
    {
        using (BotDbContext dbContext = new())
        {
            dbContext.BoardTables.Add(entity);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task Update(Table entity)
    {
        using (BotDbContext dbContext = new())
        {
            dbContext.BoardTables.Update(entity);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task Delete(Table entity)
    {
        using (BotDbContext dbContext = new())
        {
            dbContext.BoardTables.Remove(entity);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<Table> GetByNameAndBoardId(string tableName, string trelloBoardId)
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.BoardTables.
                FirstOrDefaultAsync(table => table.Name == tableName
                && table.TrelloUserBoard.TrelloBoardId == trelloBoardId);
        }
    }
}