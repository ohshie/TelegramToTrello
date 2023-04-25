using Telegram.Bot.Types;

namespace TelegramToTrello;

public class CreatingTaskDbOperations
{
    private DbOperations _dbOperations = new DbOperations();
    
     public async Task<TTTTask> AddTaskToDb(RegisteredUsers user)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            TTTTask existingTask = await _dbOperations.RetrieveUserTask(user.TelegramId);

            if (existingTask == null)
            {
                dbContext.CreatingTasks.Add(new TTTTask()
                {
                    Id = user.TelegramId,
                    TrelloId = user.TrelloId,
                    TaskName = "",
                    Tag = "",
                    TrelloBoardId = "",
                    ListId = "",
                    TaskId = "",
                    TaskDesc = "",
                    TaskPartId = "",
                    TaskPartName = "",
                    Date = ""
                });
                
                await dbContext.SaveChangesAsync();
                return existingTask;
            }
            return null;
        }
    }
     
     public async Task AddTagToTask(TTTTask userTask, string tag)
     {
         await using (BotDbContext dbContext = new BotDbContext())
         {
             userTask.Tag = tag;
             dbContext.CreatingTasks.Update(userTask);
             await dbContext.SaveChangesAsync();
         }
     }
    
    public async Task<bool> AddBoardToTask(TTTTask userTask, string boardName)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            string boardNameFetched = await _dbOperations.BoardNameToId(boardName);

            if (!string.IsNullOrEmpty(boardNameFetched))
            {
                userTask.TrelloBoardId = boardNameFetched;
                dbContext.CreatingTasks.Update(userTask);
                await dbContext.SaveChangesAsync();
                
                return true;
            }
            return false;
        }
    }

    public async Task<bool> AddTableToTask(TTTTask userTask, string listName)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            string tableId = await _dbOperations.TableNameToId(
                tableName: listName,
                telegramId: userTask.Id);

            if (!string.IsNullOrEmpty(tableId))
            {
                userTask.ListId = tableId;
                dbContext.CreatingTasks.Update(userTask);
                await dbContext.SaveChangesAsync();
                
                return true;
            }
            
            return false;
        }
    }
    
    public async Task AddPlaceholderName(TTTTask userTask)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            userTask.TaskName = "###tempname###"; 
            dbContext.CreatingTasks.Update(userTask); 
            await dbContext.SaveChangesAsync();
        }

    }
    
    public async Task AddPlaceholderDescription(TTTTask userTask)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            userTask.TaskDesc = "###tempdesc###";
            dbContext.CreatingTasks.Update(userTask);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<bool> AddPlaceholderDate(TTTTask userTask)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            userTask.Date = "###tempdate###";
            dbContext.CreatingTasks.Update(userTask);
            await dbContext.SaveChangesAsync();
            return true;
        }
    }
    

    public async Task SetTaskName(TTTTask task, string taskName)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            task.TaskName = taskName;
            task.NameSet = true;
            dbContext.CreatingTasks.Update(task);
            await dbContext.SaveChangesAsync();
        }
    }
    
    public async Task SetDescription(TTTTask task, string description)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            task.TaskDesc = description;
            task.DescSet = true;
            dbContext.CreatingTasks.Update(task);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<bool> AddParticipantToTask(TTTTask task, string participantName)
    {
        string trelloIdOfParticipant = await _dbOperations.UserNameToId(task.TrelloBoardId, participantName);
        if (trelloIdOfParticipant == null) return false;
        
        await using (BotDbContext dbContext = new BotDbContext())
        {
            task.TaskPartId = task.TaskPartId+trelloIdOfParticipant+",";
            task.TaskPartName = task.TaskPartName + participantName + ", ";
            dbContext.CreatingTasks.Update(task);
            await dbContext.SaveChangesAsync();
        }

        return true;
    }

    public async Task AddDateToTask(TTTTask task, string date)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            task.Date = date;
            
            dbContext.CreatingTasks.Update(task);
            await dbContext.SaveChangesAsync();
        }
    }
}