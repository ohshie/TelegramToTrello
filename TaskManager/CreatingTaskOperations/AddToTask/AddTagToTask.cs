using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;
using TelegramToTrello.TaskManager.CreatingTaskOperations.RequestFromuser;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

public class AddTagToTask : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private readonly TaskNameRequest _taskNameRequest;

    public AddTagToTask(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskNameRequest taskNameRequest, 
        CreatingTaskDbOperations creatingTaskDbOperations,
        Verifier verifier, BotMessenger botMessenger, TaskDbOperations taskDbOperations) : 
        base(botClient, userDbOperations, verifier, botMessenger, taskDbOperations)
    {
        _taskNameRequest = taskNameRequest;
        _creatingTaskDbOperations = creatingTaskDbOperations;
    }

    protected override async Task HandleTask(TTTTask task)
    {
        string tag = CallbackQuery.Data.Substring(CallbackList.Tag.Length).Trim();
        
        await _creatingTaskDbOperations.AddTag(task,tag);
        
        NextTask = _taskNameRequest;
        if (IsTemplate) NextTask.IsTemplate = true;
    }
}