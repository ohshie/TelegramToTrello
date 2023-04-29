namespace TelegramToTrello;

public class CreatingTaskDbOperations : DbOperations
{
    private RegisteredUser? User { get; }
    private TTTTask? UserTask { get; }
    
    public CreatingTaskDbOperations(RegisteredUser user, TTTTask userTask)
    {
        User = user;
        UserTask = userTask;
    }
    
    public async Task AddTaskToDb()
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            if (UserTask == null)
            {
                dbContext.CreatingTasks.Add(new TTTTask()
                {
                    Id = User.TelegramId,
                    TrelloId = User.TrelloId,
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
            }
        }
    }
     
     public async Task AddTagToTask(string tag)
     {
         await using (BotDbContext dbContext = new BotDbContext())
         {
             UserTask.Tag = tag;
             dbContext.CreatingTasks.Update(UserTask);
             await dbContext.SaveChangesAsync();
         }
     }
    
    public async Task<bool> AddBoardToTask(string boardName)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            string boardNameFetched = await BoardNameToId(boardName);

            if (!string.IsNullOrEmpty(boardNameFetched))
            {
                UserTask.TrelloBoardId = boardNameFetched;
                dbContext.CreatingTasks.Update(UserTask);
                await dbContext.SaveChangesAsync();
                
                return true;
            }
            return false;
        }
    }

    public async Task<bool> AddTableToTask(string listName)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            string tableId = await TableNameToId(
                tableName: listName,
                telegramId: UserTask.Id);

            if (!string.IsNullOrEmpty(tableId))
            {
                UserTask.ListId = tableId;
                dbContext.CreatingTasks.Update(UserTask);
                await dbContext.SaveChangesAsync();
                
                return true;
            }
            
            return false;
        }
    }
    
    public async Task AddPlaceholderName()
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            UserTask.TaskName = "###tempname###"; 
            dbContext.CreatingTasks.Update(UserTask); 
            await dbContext.SaveChangesAsync();
        }

    }
    
    public async Task AddPlaceholderDescription()
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            UserTask.TaskDesc = "###tempdesc###";
            dbContext.CreatingTasks.Update(UserTask);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task AddPlaceholderDate()
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            UserTask.Date = "###tempdate###";
            dbContext.CreatingTasks.Update(UserTask);
            await dbContext.SaveChangesAsync();
        }
    }
    
    public async Task SetTaskName(string taskName)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            UserTask.TaskName = taskName;
            UserTask.NameSet = true;
            dbContext.CreatingTasks.Update(UserTask);
            await dbContext.SaveChangesAsync();
        }
    }
    
    public async Task SetDescription(string description)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            UserTask.TaskDesc = description;
            UserTask.DescSet = true;
            dbContext.CreatingTasks.Update(UserTask);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<bool> AddParticipantToTask(string participantName)
    {
        string trelloIdOfParticipant = await UserNameToId(UserTask.TrelloBoardId, participantName);
        if (trelloIdOfParticipant == null) return false;
        
        await using (BotDbContext dbContext = new BotDbContext())
        {
            UserTask.TaskPartId = UserTask.TaskPartId+trelloIdOfParticipant+",";
            UserTask.TaskPartName = UserTask.TaskPartName + participantName + ",";
            dbContext.CreatingTasks.Update(UserTask);
            await dbContext.SaveChangesAsync();
        }

        return true;
    }

    public async Task AddDateToTask(string date)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            UserTask.Date = date;
            
            dbContext.CreatingTasks.Update(UserTask);
            await dbContext.SaveChangesAsync();
        }
    }
}