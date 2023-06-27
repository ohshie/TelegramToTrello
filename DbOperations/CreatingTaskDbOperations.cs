namespace TelegramToTrello;

public class CreatingTaskDbOperations : DbOperations
{
    private RegisteredUser? User { get; }
    private TTTTask UserTask { get; }
    
    private readonly TTTTaskRepository _taskRepository = new();
    
    public CreatingTaskDbOperations(RegisteredUser user, TTTTask userTask)
    {
        User = user;
        UserTask = userTask;
    }
    
    public async Task CreateTask()
    {
        if (UserTask == null)
        {
            TTTTask newTask = new TTTTask()
            {
                Id = User.TelegramId,
                TrelloId = User.TrelloId,
            };

            await _taskRepository.Add(newTask);
        }
    }
     
    public async Task AddTag(string tag)
    {
        UserTask.Tag = tag;
        await _taskRepository.Update(UserTask);
    } 
    
    public async Task<string?> AddBoard(string boardId)
    {
        BoardRepository boardRepository = new BoardRepository();
        Board? board = await boardRepository.Get(boardId);

        if (board != null)
        {
            UserTask.TrelloBoardId = board.TrelloBoardId;
            UserTask.TrelloBoardName = board.BoardName;

            await _taskRepository.Update(UserTask);

            return board.BoardName;
        }

        return null;
    }

    public async Task<bool> AddTable(string tableName)
    {
        TableRepository tableRepository = new TableRepository();
        var table = await tableRepository.GetByNameAndBoardId(tableName: tableName, 
            trelloBoardId: UserTask.TrelloBoardId);

        if (table != null)
        {
            UserTask.ListId = table.TableId;
            await _taskRepository.Update(UserTask);

            return true;
        }

        return false;
    }
    
    
    
    public async Task AddPlaceholderName()
    {
        UserTask.TaskName = "###tempname###";
        await _taskRepository.Update(UserTask);
    }
    
    public async Task AddPlaceholderDescription()
    {
        UserTask.TaskDesc = "###tempdesc###";
        await _taskRepository.Update(UserTask);
    }

    public async Task AddPlaceholderDate()
    {
        UserTask.Date = "###tempdate###";
        await _taskRepository.Update(UserTask);
    }
    
    public async Task AddName(string taskName)
    {
        UserTask.TaskName = taskName;
        await _taskRepository.Update(UserTask);
    }
    
    public async Task AddDescription(string description)
    {
        UserTask.TaskDesc = description;
        await _taskRepository.Update(UserTask);
    }

    public async Task<bool> AddParticipant(string participantName)
    {
        TrelloUsersRepository usersRepository = new TrelloUsersRepository();
        var participant = await usersRepository.GetByNameAndBoardId(participantName, UserTask.TrelloBoardId);
        if (participant == null) return false;
        
        UserTask.TaskPartId = UserTask.TaskPartId+participant.TrelloUserId+",";
        UserTask.TaskPartName = UserTask.TaskPartName + participantName + ",";

        TTTTaskRepository taskRepository = new();
        await taskRepository.Update(UserTask);
        return true;
        
        // string trelloIdOfParticipant = await UserNameToId(UserTask.TrelloBoardId, participantName);
        // if (trelloIdOfParticipant == null) return false;
        //
        // await using (BotDbContext dbContext = new BotDbContext())
        // {
        //     UserTask.TaskPartId = UserTask.TaskPartId+trelloIdOfParticipant+",";
        //     UserTask.TaskPartName = UserTask.TaskPartName + participantName + ",";
        //     dbContext.CreatingTasks.Update(UserTask);
        //     await dbContext.SaveChangesAsync();
        // }
        //
        // return true;
    }

    public async Task AddDate(string date)
    {
        UserTask.Date = date;
        await _taskRepository.Update(UserTask);
    }
    
    public async Task MarkMessageForDeletion(int messageId)
    {
        UserTask.LastBotMessage = messageId;
        await _taskRepository.Update(UserTask);
    }
}