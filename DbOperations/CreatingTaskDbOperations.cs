using TelegramToTrello.Repositories;

namespace TelegramToTrello;

public class CreatingTaskDbOperations
{
    private readonly IRepository<TTTTask> _taskRepository;
    private readonly ITrelloUsersRepository _trelloUsersRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IRepository<Board> _boardRepository;

    public CreatingTaskDbOperations(IRepository<TTTTask> taskRepository, 
        ITrelloUsersRepository trelloUsersRepository,
        ITableRepository tableRepository,
        IRepository<Board> boardRepository)
    {
        _taskRepository = taskRepository;
        _trelloUsersRepository = trelloUsersRepository;
        _tableRepository = tableRepository;
        _boardRepository = boardRepository;
    }

    public async Task CreateTask(RegisteredUser user)
    {
            TTTTask newTask = new TTTTask()
            {
                Id = user.TelegramId,
                TrelloId = user.TrelloId,
            };

            await _taskRepository.Add(newTask);
    }
     
    public async Task AddTag(TTTTask userTask ,string tag)
    {
        userTask.Tag = tag;
        await _taskRepository.Update(userTask);
    } 
    
    public async Task<string?> AddBoard(TTTTask userTask, string boardId)
    {
        Board? board = await _boardRepository.Get(boardId);

        if (board != null)
        {
            userTask.TrelloBoardId = board.TrelloBoardId;
            userTask.TrelloBoardName = board.BoardName;

            await _taskRepository.Update(userTask);

            return board.BoardName;
        }

        return null;
    }

    public async Task<bool> AddTable(TTTTask userTask, string tableName)
    {
        var table = await _tableRepository.GetByNameAndBoardId(tableName: tableName, 
            trelloBoardId: userTask.TrelloBoardId);

        if (table != null)
        {
            userTask.ListId = table.TableId;
            await _taskRepository.Update(userTask);

            return true;
        }

        return false;
    }
    
    public async Task AddPlaceholderName(TTTTask userTask)
    {
        userTask.TaskName = "###tempname###";
        await _taskRepository.Update(userTask);
    }
    
    public async Task AddPlaceholderDescription(TTTTask userTask)
    {
        userTask.TaskDesc = "###tempdesc###";
        await _taskRepository.Update(userTask);
    }

    public async Task AddPlaceholderDate(TTTTask userTask)
    {
        userTask.Date = "###tempdate###";
        await _taskRepository.Update(userTask);
    }

    public async Task WaitingForAttachmentToggle(TTTTask userTask)
    {
        userTask.WaitingForAttachment = !userTask.WaitingForAttachment;
        await _taskRepository.Update(userTask);
    }
    
    public async Task AddName(TTTTask userTask,string taskName)
    {
        userTask.TaskName = taskName;
        await _taskRepository.Update(userTask);
    }
    
    public async Task AddDescription(TTTTask userTask, string description)
    {
        userTask.TaskDesc = description;
        await _taskRepository.Update(userTask);
    }

    public async Task<bool> AddParticipant(TTTTask userTask, string participantName)
    {
        var participant = await _trelloUsersRepository.GetByNameAndBoardId(participantName, userTask.TrelloBoardId);
        if (participant == null) return false;
        
        userTask.TaskPartId = userTask.TaskPartId+participant.TrelloUserId+",";
        userTask.TaskPartName = userTask.TaskPartName + participantName + ",";
        
        await _taskRepository.Update(userTask);
        return true;
    }

    public async Task AddDate(TTTTask userTask, string date)
    {
        userTask.Date = date;
        await _taskRepository.Update(userTask);
    }
    
    public async Task MarkMessage(TTTTask userTask, int messageId)
    {
        userTask.LastBotMessage = messageId;
        await _taskRepository.Update(userTask);
    }

    public async Task AddFilePath(TTTTask userTask, string filePath)
    {
        userTask.Attachments += filePath + ", ";
        await _taskRepository.Update(userTask);
    }
}