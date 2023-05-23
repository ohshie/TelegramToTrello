using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.Dboperations;

namespace TelegramToTrello;

public class DbOperations
{
    public async Task<bool> RegisterNewUser(Message message)
    {
        await using BotDbContext dbContext = new BotDbContext();
        {
            RegisteredUser existingUser = await dbContext.Users.FindAsync((int)message.From.Id);

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

    public async Task AddTrelloTokenAndId(string token, string trelloId, int telegramId)
    {
        await using BotDbContext dbContext = new BotDbContext();
        {
            RegisteredUser existingUser = await dbContext.Users.FindAsync(telegramId);

            if (existingUser != null)
            {
                existingUser.TrelloToken = token;
                existingUser.TrelloId = trelloId;

                await dbContext.SaveChangesAsync();
            }
        }
    }
    
    public async Task<bool> LinkBoardsFromTrello(int telegramId)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            RegisteredUser trelloUser = await dbContext.Users.FindAsync(telegramId);
            if (trelloUser == null) return false;
            if (trelloUser.TrelloId == "") return false;
            
            WriteFromTrelloToDb writeFromTrelloToDb = new WriteFromTrelloToDb();
            await writeFromTrelloToDb.PopulateDbWithBoardsUsersTables(trelloUser);

            return true;
        }
    }
    
    public async Task<RegisteredUser?> RetrieveTrelloUser(int telegramId)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            RegisteredUser trelloUser = await dbContext.Users
                .Include(ru => ru.UsersBoards)
                .ThenInclude(ub => ub.Boards)
                .FirstOrDefaultAsync(um => um.TelegramId == telegramId);

            if (trelloUser != null)
            {
                return trelloUser;
            }
        }
        return null;
    }

    public async Task<Board> RetrieveBoard(int telegramId, string boardName)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            RegisteredUser trelloUser = await dbContext.Users
                .Include(ru => ru.UsersBoards)
                .ThenInclude(ub => ub.Boards)
                .ThenInclude(b => b.Tables)
                .Include(ru => ru.UsersBoards)
                .ThenInclude(ub => ub.Boards)
                .ThenInclude(b => b.UsersOnBoards)
                .FirstOrDefaultAsync(um => um.TelegramId == telegramId);
            
            if (trelloUser != null)
            {
                return trelloUser.UsersBoards
                    .Select(ub => ub.Boards)
                    .FirstOrDefault(b => b.TrelloBoardId == boardName);
            }
        }
        return null;
    }

    public async Task<TTTTask> RetrieveUserTask(int telegramId)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            TTTTask userCreatedTask = await dbContext.CreatingTasks.FindAsync(telegramId);

            if (userCreatedTask != null)
            {
                return userCreatedTask;
            }
        }
        return null;
    }
    
    internal async Task<Board?> CheckIfBoardExist(string boardId)
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

    internal async Task<string> TableNameToId(string tableName, int telegramId)
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

    internal async Task<string> UserNameToId(string boardName, string userName)
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

    public async Task RemoveEntry(TTTTask userTask)
    {
        using (BotDbContext dbContext = new BotDbContext())
        {
            dbContext.RemoveRange(userTask);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task ToggleEditModeForTask(TTTTask userTask)
    {
        using (BotDbContext dbContext = new())
        {
            userTask.InEditMode = !userTask.InEditMode;
            dbContext.CreatingTasks.Update(userTask);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task ResetParticipants(TTTTask userTask)
    {
        using (BotDbContext dbContext = new())
        {
            userTask.TaskPartId = null;
            userTask.TaskPartName = null;

            dbContext.CreatingTasks.Update(userTask);
            await dbContext.SaveChangesAsync();
        }
    }
}