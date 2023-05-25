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
    
    public async Task CreateTask()
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            if (UserTask == null)
            {
                dbContext.CreatingTasks.Add(new TTTTask()
                {
                    Id = User.TelegramId,
                    TrelloId = User.TrelloId,
                });
                
                await dbContext.SaveChangesAsync();
            }
        }
    }
     
    public async Task AddTag(string tag)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            UserTask.Tag = tag;
            dbContext.CreatingTasks.Update(UserTask);
            await dbContext.SaveChangesAsync();
        }
    } 
    
    public async Task<string?> AddBoard(string boardId)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            Board? board = await CheckIfBoardExist(boardId);
            if (board != null)
            {
                UserTask.TrelloBoardId = board.TrelloBoardId;
                UserTask.TrelloBoardName = board.BoardName;
                dbContext.CreatingTasks.Update(UserTask);
                await dbContext.SaveChangesAsync();
                
                return board.BoardName;
            }
            return null;
        }
    }

    public async Task<bool> AddTable(string listName)
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
    
    public async Task AddName(string taskName)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            UserTask.TaskName = taskName;
            dbContext.CreatingTasks.Update(UserTask);
            await dbContext.SaveChangesAsync();
        }
    }
    
    public async Task AddDescription(string description)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            UserTask.TaskDesc = description;
            dbContext.CreatingTasks.Update(UserTask);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<bool> AddParticipant(string participantName)
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

    public async Task AddDate(string date, int messageId)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            UserTask.Date = date;

            dbContext.CreatingTasks.Update(UserTask);
            await dbContext.SaveChangesAsync();
        }
    }
    
    public async Task MarkMessageForDeletion(int messageId)
    {
        using (BotDbContext dbContext = new())
        {
            UserTask.MessageForDeletionId = messageId;

            dbContext.CreatingTasks.Update(UserTask);
            await dbContext.SaveChangesAsync();
        }
    }
}