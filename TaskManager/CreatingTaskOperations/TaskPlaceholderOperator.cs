using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class TaskPlaceholderOperator
{
    private readonly TaskDbOperations _taskDbOperations;
    private readonly ITelegramBotClient _botClient;
    private readonly AddNameToTask _addNameToTask;
    private readonly AddDescriptionToTask _addDescriptionToTask;
    private readonly AddDateToTask _addDateToTask;
    private readonly UserDbOperations _userDbOperations;

    public TaskPlaceholderOperator(UserDbOperations userDbOperations, 
        TaskDbOperations taskDbOperations, 
        ITelegramBotClient botClient,
        AddNameToTask addNameToTask,
        AddDescriptionToTask addDescriptionToTask,
        AddDateToTask addDateToTask)
    {
        _userDbOperations = userDbOperations;
        _taskDbOperations = taskDbOperations;
        _botClient = botClient;
        _addNameToTask = addNameToTask;
        _addDescriptionToTask = addDescriptionToTask;
        _addDateToTask = addDateToTask;
    }

    public async Task SortMessage(Message message)
    {
        TTTTask? task = await GetTrelloUserAndTask(message);
        if (task == null) return;
        
        if (task.TaskName == "###tempname###")
        {
            await _addNameToTask.Execute(message);
        }
        
        if (task.TaskDesc == "###tempdesc###")
        {
            await _addDescriptionToTask.Execute(message);
        }
        
        if (task.Date == "###tempdate###")
        {
            await _addDateToTask.Execute(message);
        }
    }
    
    private async Task<TTTTask?> GetTrelloUserAndTask(Message message)
    {
        RegisteredUser ?trelloUser = await _userDbOperations.RetrieveTrelloUser((int)message.Chat.Id);
        if (trelloUser != null)
        {   
            var task = await _taskDbOperations.RetrieveUserTask((int)message.Chat.Id);
            return task;
        }

        return null;
    }
}