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
        RegisteredUser trelloUser = await _dbContext.Users
                .Include(ru => ru.Boards)
                .ThenInclude(b => b.Tables)
                .Include(ru => ru.Boards)
                .ThenInclude(b => b.UsersOnBoards)
                .FirstOrDefaultAsync(um => um.TelegramId == telegramId);
            
            if (trelloUser != null)
            {
                return trelloUser.Boards!.FirstOrDefault(b => b.TrelloBoardId == boardName);
            }
        return null;
    }
}