using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.Dboperations;

namespace TelegramToTrello;

public class DbOperations
{
    public async Task<bool> LinkTelegramToTrello(Message message, ITelegramBotClient botClient, string trelloID, string trelloUserName)
    {
        using BotDbContext dbContext = new BotDbContext();
        var existingUser = await dbContext.TrelloUsers.FindAsync((int)message.From.Id);
        
        if (existingUser == null)
        {
            dbContext.TrelloUsers.Add(new TrelloUser
            {
                Id = (int)message.From.Id,
                TelegramUserName = message.From.Username,
                TrelloUserName = trelloUserName,
                TrelloId = trelloID
            });

            await dbContext.SaveChangesAsync();

            return true;
        }
        
        await botClient.SendTextMessageAsync(message.Chat.Id, $"Account {existingUser.TelegramUserName} already linked");
        return false;
    }

    public async Task<bool> LinkBoardsFromTrello(int telegramId)
    {
        using (BotDbContext dbContext = new BotDbContext())
        {
            TrelloUser trelloUser = await dbContext.TrelloUsers.FindAsync(telegramId);
            if (trelloUser == null) return false;
            
            WriteFromTrelloToDb writeFromTrelloToDb = new WriteFromTrelloToDb();
            await writeFromTrelloToDb.PopulateDbWithBoards(trelloUser, telegramId);

            return true;
        }
    }
    
    public async Task<bool> ClearTask(int telegramId)
    {
        using BotDbContext dbContext = new BotDbContext();
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
    
    public async Task<TrelloUser?> RetrieveTrelloUser(int telegramId)
    {
        using (BotDbContext dbContext = new BotDbContext())
        {
            TrelloUser trelloUser = await dbContext.TrelloUsers.Include(um => um.TrelloUserBoards)
                .ThenInclude(tub => tub.TrelloBoardTables)
                .FirstOrDefaultAsync(um => um.Id == telegramId);

            if (trelloUser != null)
            {
                return trelloUser;
            }
        }
        return null;
    }

    public async Task<TTTTask> RetrieveUserTask(int telegramId)
    {
        using (BotDbContext dbContext = new BotDbContext())
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
        using (BotDbContext dbContext = new BotDbContext())
        {
            TTTTask tttTask = await dbContext.CreatingTasks.FirstOrDefaultAsync(um => um.Id == telegramId);
        
            if (tttTask == null)
            {
                return false;
            }
            
            return true;
        }
    }

    public async Task<string> BoardNameToId(string boardName, int telegramId)
    {
        using (BotDbContext dbContext = new BotDbContext())
        {
            TrelloUser trelloUser = await dbContext.TrelloUsers.FindAsync(telegramId);
            
            TrelloUserBoard boardNameToId = await dbContext.TrelloUserBoards.FirstOrDefaultAsync(tub =>
                tub.Name == boardName && tub.TrelloUserId == trelloUser.TrelloId);

            if (boardNameToId != null)
            {
                return boardNameToId.TrelloBoardId;
            }
                
            return null;
        }
    }

    public async Task<string> TableNameToId(string tableName, int telegramId)
    {
        using (BotDbContext dbContext = new BotDbContext())
        {
            TTTTask task = await dbContext.CreatingTasks.FindAsync(telegramId);
            
            TrelloBoardTable tableNameToId = await dbContext.BoardTables.FirstOrDefaultAsync(bt =>
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
        using (BotDbContext dbContext = new BotDbContext())
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