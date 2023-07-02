using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class TaskPlaceholderOperator
{
    private readonly TaskDbOperations _taskDbOperations;
    private readonly AddNameToTask _addNameToTask;
    private readonly AddDescriptionToTask _addDescriptionToTask;
    private readonly AddDateToTask _addDateToTask;
    private readonly AddAttachmentToTask _addAttachmentToTask;
    private readonly UserDbOperations _userDbOperations;

    public TaskPlaceholderOperator(UserDbOperations userDbOperations, 
        TaskDbOperations taskDbOperations,
        AddNameToTask addNameToTask,
        AddDescriptionToTask addDescriptionToTask,
        AddDateToTask addDateToTask,
        AddAttachmentToTask addAttachmentToTask)
    {
        _userDbOperations = userDbOperations;
        _taskDbOperations = taskDbOperations;
        _addNameToTask = addNameToTask;
        _addDescriptionToTask = addDescriptionToTask;
        _addDateToTask = addDateToTask;
        _addAttachmentToTask = addAttachmentToTask;
    }

    public async Task SortMessage(Message message)
    {
        TTTTask? task = await GetTrelloUserAndTask(message);
        if (task == null) return;
        
        if (task.TaskName == "###tempname###")
        {
            await _addNameToTask.Execute(message);
            return;
        }
        
        if (task.TaskDesc == "###tempdesc###")
        {
            await _addDescriptionToTask.Execute(message);
            return;
        }
        
        if (task.Date == "###tempdate###")
        {
            await _addDateToTask.Execute(message);
            return;
        }

        if (task.WaitingForAttachment)
        {
            if (message.Photo is not null || message.Document is not null)
            {
                await _addAttachmentToTask.Execute(message);
                return;
            }
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