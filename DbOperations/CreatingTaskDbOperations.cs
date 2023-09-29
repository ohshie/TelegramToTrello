using TelegramToTrello.Repositories;

namespace TelegramToTrello;

public class CreatingTaskDbOperations
{
    private readonly IUsersRepository _userRepository;
    private readonly ITTTTaskRepository _taskRepository;
    private readonly ITrelloUsersRepository _trelloUsersRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IRepository<Board> _boardRepository;

    public CreatingTaskDbOperations(ITTTTaskRepository taskRepository, 
        ITrelloUsersRepository trelloUsersRepository,
        ITableRepository tableRepository,
        IRepository<Board> boardRepository, 
        IUsersRepository userRepository)
    {
        _taskRepository = taskRepository;
        _trelloUsersRepository = trelloUsersRepository;
        _tableRepository = tableRepository;
        _boardRepository = boardRepository;
        _userRepository = userRepository;
    }

    public async Task CreateTask(int userId)
    {
        var user = await _userRepository.Get(userId);
        
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

    public async Task AddTable(TTTTask userTask, string tableName)
    {
        var table = await _tableRepository.GetByNameAndBoardId(tableName: tableName, 
            trelloBoardId: userTask.TrelloBoardId);

        if (table != null)
        {
            userTask.ListId = table.TableId;
            await _taskRepository.Update(userTask);
        }
    }
    
    public async Task AddPlaceholderName(TTTTask userTask, bool isTemplate = false)
    {
        if (isTemplate)
        {
            userTask.TaskName += " ##template##"; 
        }
        else
        {
            userTask.TaskName = "###tempname###";
        }
        
        await _taskRepository.Update(userTask);
    }
    
    public async Task AddPlaceholderDescription(TTTTask userTask, bool isTemplate = false)
    {
        if (isTemplate)
        {
            userTask.TaskDesc += "##template##";
        }
        else
        {
            userTask.TaskDesc = "###tempdesc###";
        }
        
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
    
    public async Task AddName(TTTTask userTask, string taskName, bool isTemplate = false)
    {
        if (isTemplate)
        {
            userTask.TaskName = userTask.TaskName
                .Substring(0, userTask.TaskName.Length - " ##template##".Length)
                .Trim();
            userTask.TaskName += " "+taskName;
        }
        else
        {
            userTask.TaskName = taskName;
        }
        
        await _taskRepository.Update(userTask);
    }
    
    public async Task AddDescription(TTTTask userTask, string description, bool isTemplate = false)
    {
        if (isTemplate)
        {
            userTask.TaskDesc = userTask.TaskDesc
                .Substring(0, userTask.TaskDesc.Length - "##template##".Length)
                .Trim();
            userTask.TaskDesc += " \n" + description;
        }
        else
        {
            userTask.TaskDesc = description;
        }
        
        await _taskRepository.Update(userTask);
    }

    public async Task AddParticipant(TTTTask userTask, string participantName)
    {
        var participant = await _trelloUsersRepository.GetByNameAndBoardId(participantName, userTask.TrelloBoardId);
        
        userTask.TaskPartId = userTask.TaskPartId+participant.TrelloUserId+",";
        userTask.TaskPartName = userTask.TaskPartName + participantName + ",";
        
        await _taskRepository.Update(userTask);
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