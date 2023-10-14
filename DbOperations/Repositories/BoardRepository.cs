using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello;

public class BoardRepository : IBoardRepository
{
    private readonly BotDbContext _dbContext;

    public BoardRepository(BotDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<Board> Get(int id)
    {
        var board = await _dbContext.Boards.FirstOrDefaultAsync(b => b.Id == id);

        await _dbContext.Entry(board).Collection(b => b.Users).LoadAsync();
        await _dbContext.Entry(board).Collection(b => b.Tables).LoadAsync();

        return board;
    }

    public async Task<Board> Get(string id)
    {
        var board = await _dbContext.Boards.FirstOrDefaultAsync(b => b.TrelloBoardId == id);

        await _dbContext.Entry(board).Collection(b => b.Users).LoadAsync();
        await _dbContext.Entry(board).Collection(b => b.Tables).LoadAsync();

        return board;
    }

    public async Task<IEnumerable<Board>> GetAll()
    {
        return await _dbContext.Boards
            .Include(b=> b.Users)
            .Include(b=> b.Tables)
            .ToListAsync();
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

    public async Task DeleteRange(IEnumerable<Board> entity)
    {
        _dbContext.Boards.RemoveRange(entity);
        await _dbContext.SaveChangesAsync();
    }
}