using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello.Repositories;

public class TableRepository : ITableRepository
{
    private readonly BotDbContext _dbContext;

    public TableRepository(BotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Table> Get(int id)
    { 
        return await _dbContext.BoardTables.FindAsync(id);
    }
    
    public async Task<Table> Get(string id)
    {
        return await _dbContext.BoardTables.FirstOrDefaultAsync(b => b.TableId == id);
    }

    public async Task<IEnumerable<Table>> GetAll()
    {
        return await _dbContext.BoardTables
            .Include(t => t.TrelloUserBoard)
            .ToListAsync();
    }

    public async Task Add(Table entity)
    {
        _dbContext.BoardTables.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Update(Table entity)
    {
        _dbContext.BoardTables.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Delete(Table entity)
    {
        _dbContext.BoardTables.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Table> GetByNameAndBoardId(string tableName, string trelloBoardId)
    {
        return await _dbContext.BoardTables.
                FirstOrDefaultAsync(table => table.Name == tableName
                && table.TrelloUserBoard.TrelloBoardId == trelloBoardId);
    }

    public async Task AddRange(IEnumerable<Table> entity)
    {
        _dbContext.BoardTables.AddRange(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveRange(IEnumerable<Table> entity)
    {
        _dbContext.BoardTables.RemoveRange(entity);
        await _dbContext.SaveChangesAsync();
    }
}