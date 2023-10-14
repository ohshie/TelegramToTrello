using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello;

public class DbOperations
{
    private readonly BotDbContext _dbContext;

    public DbOperations(BotDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<Board?> RetrieveBoard(int telegramId, string boardName)
    {
        var board = await _dbContext.Users
            .Where(u => u.TelegramId == telegramId)
            .SelectMany(u => u.Boards)
            .FirstOrDefaultAsync(b => b.TrelloBoardId == boardName);
        
        if (board == null) return board;
        
        await _dbContext.Entry(board)
                .Collection(b => b.Tables)
                .LoadAsync();

        await _dbContext.Entry(board)
                .Collection(b => b.UsersOnBoards)
                .LoadAsync();

        return board;
    }
}