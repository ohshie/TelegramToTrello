using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.Dboperations;

namespace TelegramToTrello;

public class DbOperations
{
    public async Task<bool> RegisterNewUser(Message message, ITelegramBotClient botClient)
    {
        await using BotDbContext dbContext = new BotDbContext();
        {
            RegisteredUsers existingUser = await dbContext.Users.FindAsync((int)message.From.Id);

            if (existingUser == null)
            {
                dbContext.Users.Add(new RegisteredUsers
                {
                    TelegramId = (int)message.From.Id,
                    TelegramName = message.From.Username,
                    TrelloId = "",
                    TrelloToken = "",
                });
                
                await dbContext.SaveChangesAsync();
                return true;
            }
            await botClient.SendTextMessageAsync(message.Chat.Id, $"Account {existingUser.TrelloId} already linked");
        }
        return false;
    }

    public async Task<bool> AddTrelloTokenAndId(string token, string trelloId, int telegramId)
    {
        await using BotDbContext dbContext = new BotDbContext();
        {
            RegisteredUsers existingUser = await dbContext.Users.FindAsync(telegramId);

            if (existingUser != null)
            {
                existingUser.TrelloToken = token;
                existingUser.TrelloId = trelloId;

                await dbContext.SaveChangesAsync();
                return true;
            }
        }
        return false;
    }
    
    public async Task<bool> LinkBoardsFromTrello(int telegramId)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            RegisteredUsers trelloUser = await dbContext.Users.FindAsync(telegramId);
            if (trelloUser == null) return false;
            if (trelloUser.TrelloId == "") return false;
            
            WriteFromTrelloToDb writeFromTrelloToDb = new WriteFromTrelloToDb();
            await writeFromTrelloToDb.PopulateDbWithBoards(trelloUser);

            return true;
        }
    }
    
    public async Task<bool> ClearTask(int telegramId)
    {
        await using BotDbContext dbContext = new BotDbContext();
        {
            TTTTask taskToDelete = await dbContext.CreatingTasks.FindAsync(telegramId);

            if (taskToDelete != null)
            {
                dbContext.CreatingTasks.Remove(taskToDelete);
                await dbContext.SaveChangesAsync();
                return true;
            }
        }
        return false;
    }
    
    public async Task<RegisteredUsers?> RetrieveTrelloUser(int telegramId)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            RegisteredUsers trelloUser = await dbContext.Users
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

    public async Task<Boards> RetrieveBoards(int telegramId, string boardName)
    {
        string checkIfId = await BoardNameToId(boardName);
        if (checkIfId != null) boardName = checkIfId;
        
        await using (BotDbContext dbContext = new BotDbContext())
        {
            RegisteredUsers trelloUser = await dbContext.Users
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

    public async Task<bool> CheckIfUserAlreadyCreatingTask(int telegramId)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            TTTTask tttTask = await dbContext.CreatingTasks.FirstOrDefaultAsync(um => um.Id == telegramId);
        
            if (tttTask == null)
            {
                return false;
            }
            
            return true;
        }
    }

    public async Task<string> BoardNameToId(string boardName)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            Boards board = await dbContext.Boards.FirstOrDefaultAsync(b => b.BoardName == boardName);
            
            if (board != null)
            {
                return board.TrelloBoardId;
            }
                
            return null;
        }
    }

    public async Task<string> TableNameToId(string tableName, int telegramId)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            TTTTask task = await dbContext.CreatingTasks.FindAsync(telegramId);
            
            Tables tableNameToId = await dbContext.BoardTables.FirstOrDefaultAsync(bt =>
                bt.Name == tableName && bt.TrelloUserBoard.TrelloBoardId == task.BoardId);

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
    
    public async Task AddTaskIdToCreatedTasks(int telegramId, string taskId)
    {
        using (BotDbContext dbContext = new BotDbContext())
        {
            TTTTask task = await dbContext.CreatingTasks.FindAsync(telegramId);
            if (task != null)
            {
                task.TaskId = taskId;
                dbContext.SaveChangesAsync();
            }
        }
    }
}