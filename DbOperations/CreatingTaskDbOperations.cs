using Telegram.Bot.Types;

namespace TelegramToTrello;

public class CreatingTaskDbOperations
{
    private DbOperations _dbOperations = new DbOperations();
    
     public async Task<bool> AddTaskToDb(int telegramId)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            TTTTask existingTask = await _dbOperations.RetrieveUserTask(telegramId);

            if (existingTask == null)
            {
                dbContext.CreatingTasks.Add(new TTTTask()
                {
                    Id = telegramId,
                    TaskName = "",
                    Tag = "",
                    BoardId = "",
                    ListId = "",
                    TaskId = "",
                    TaskDesc = "",
                    TaskPartId = "",
                    TaskPartName = "",
                    Date = ""
                });
                
                await dbContext.SaveChangesAsync();
                return false;
            }
            return true;
        }
    }
     
     public async Task<bool> AddTagToTask(int telegramId, string tag)
     {
         await using (BotDbContext dbContext = new BotDbContext())
         {
             TTTTask task = await _dbOperations.RetrieveUserTask(telegramId);
             if (task == null) return false;
             
             task.Tag = tag;
             dbContext.CreatingTasks.Update(task);
             await dbContext.SaveChangesAsync();
             return true;
         }
     }
    
    public async Task<bool> AddBoardToTask(int telegramId, string boardName)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            TTTTask task = await _dbOperations.RetrieveUserTask(telegramId);
            if (task == null) return false;
            
            string boardNameFetched = await _dbOperations.BoardNameToId(boardName);

            if (!string.IsNullOrEmpty(boardNameFetched))
            {
                task.BoardId = boardNameFetched;
                dbContext.CreatingTasks.Update(task);
                await dbContext.SaveChangesAsync();
                
                return true;
            }
            return false;
        }
    }

    public async Task<bool> AddTableToTask(int telegramId, string listName)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            TTTTask task = await _dbOperations.RetrieveUserTask(telegramId);
            
            string tableId = await _dbOperations.TableNameToId(
                tableName: listName,
                telegramId: task.Id);

            if (!string.IsNullOrEmpty(tableId))
            {
                task.ListId = tableId;
                dbContext.CreatingTasks.Update(task);
                await dbContext.SaveChangesAsync();
                
                return true;
            }
            
            return false;
        }
    }
    
    public async Task<bool> AddPlaceholderName(int telegramId)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            TTTTask task = await _dbOperations.RetrieveUserTask(telegramId);
            
            if (task != null)
            {
                if (string.IsNullOrEmpty(task.TaskName))
                {
                    task.TaskName = "###tempname###";
                }

                dbContext.CreatingTasks.Update(task);
                await dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
    
    public async Task<bool> AddPlaceholderDescription(int telegramId)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            TTTTask task = await _dbOperations.RetrieveUserTask(telegramId);
            
            if (task != null)
            {
                if (string.IsNullOrEmpty(task.TaskDesc))
                {
                    task.TaskDesc = "###tempdesc###";
                }
                
                dbContext.CreatingTasks.Update(task);
                await dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
    
    public async Task<bool> AddPlaceholderDate(int telegramId)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            TTTTask task = await _dbOperations.RetrieveUserTask(telegramId);
            
            if (task != null)
            {
                task.Date = "###tempdate###";
                dbContext.CreatingTasks.Update(task); 
                await dbContext.SaveChangesAsync(); 
                return true;
            }
            return false;
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
        string trelloIdOfParticipant = await _dbOperations.UserNameToId(task.BoardId, participantName);
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