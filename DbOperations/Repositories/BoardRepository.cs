using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello;

public class BoardRepository : IRepository<Board>
{
    private readonly BotDbContext _dbContext;

    public BoardRepository(BotDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<Board> Get(int id)
    {
        return await _dbContext.Boards.FindAsync(id);
    }

    public async Task<Board> Get(string id)
    {
        return await _dbContext.Boards.FirstOrDefaultAsync(b => b.TrelloBoardId == id);
    }

    public async Task<IEnumerable<Board>> GetAll()
    {
        return await _dbContext.Boards.ToListAsync();
    }

    public async Task Add(Board entity)
    {
        _dbContext.Boards.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Update(Board entity)
    {
        _dbContext.Boards.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Delete(Board entity)
    {
        _dbContext.Boards.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }
}