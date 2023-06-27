using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello;

public class DbOperations
{
    public async Task<Board?> RetrieveBoard(int telegramId, string boardName)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            RegisteredUser trelloUser = await dbContext.Users
                .Include(ru => ru.Boards)
                .ThenInclude(b => b.Tables)
                .Include(ru => ru.Boards)
                .ThenInclude(b => b.UsersOnBoards)
                .FirstOrDefaultAsync(um => um.TelegramId == telegramId);
            
            if (trelloUser != null)
            {
                return trelloUser.Boards!.FirstOrDefault(b => b.TrelloBoardId == boardName);
            }
        }
        return null;
    }

    public async Task<Board?> CheckIfBoardExist(string boardId)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            Board board = await dbContext.Boards.FirstOrDefaultAsync(b => b.TrelloBoardId == boardId);
            
            if (board != null)
            {
                return board;
            }
            
            return null;
        }
    }

    public async Task<string> TableNameToId(string tableName, int telegramId)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            TTTTask task = await dbContext.CreatingTasks.FindAsync(telegramId);
            
            Table tableNameToId = await dbContext.BoardTables.FirstOrDefaultAsync(bt =>
                bt.Name == tableName && bt.TrelloUserBoard.TrelloBoardId == task.TrelloBoardId);

            if (tableNameToId != null)
            {
                return tableNameToId.TableId;
            }
        }
        return null;
    }

    public async Task<string> UserNameToId(string boardName, string userName)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            UsersOnBoard user = await dbContext.UsersOnBoards
                .Include(uob => uob.TrelloBoard)
                .FirstOrDefaultAsync(uob => uob.Name == userName 
                                            && uob.TrelloBoard.TrelloBoardId == boardName);
            if (user != null)
            {
                return user.TrelloUserId;
            }
        }
        return null;
    }
}