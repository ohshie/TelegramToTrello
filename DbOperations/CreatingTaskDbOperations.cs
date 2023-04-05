namespace TelegramToTrello;

public class CreatingTaskDbOperations
{
    private DbOperations _dbOperations = new DbOperations();
    
     public async Task<bool> AddTaskToDb(string taskName, int telegramId)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            TTTTask existingTask = await _dbOperations.RetrieveUserTask(telegramId);

            if (existingTask == null)
            {
                dbContext.CreatingTasks.Add(new TTTTask()
                {
                    Id = telegramId,
                    TaskName = taskName,
                    Tag = "",
                    BoardId = "",
                    ListId = "",
                    TaskId = "",
                    TaskDesc = "",
                    TaskCurrentParticipant = "",
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

    public async Task AddDescriptionToTask(TTTTask task, string description)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            task.TaskDesc = description;
            
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<bool> AddParticipantToTask(TTTTask task, string participantName)
    {
        string trelloIdOfParticipant = await _dbOperations.UserNameToId(task.BoardId, participantName);
        if (trelloIdOfParticipant == null) return false;
        
        await using (BotDbContext dbContext = new BotDbContext())
        {
            task.TaskCurrentParticipant = trelloIdOfParticipant;
            
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