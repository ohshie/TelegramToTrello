using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;
using TelegramToTrello.TaskManager.CreatingTaskOperations.RequestFromuser;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

public class AddNameToTask : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private readonly DisplayCurrentTaskInfo _displayCurrentTaskInfo;

    public AddNameToTask(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations, TaskDescriptionRequest taskDescriptionRequest,
        CreatingTaskDbOperations creatingTaskDbOperations,
        DisplayCurrentTaskInfo displayCurrentTaskInfo, Verifier verifier) : base(botClient, userDbOperations, taskDbOperations, verifier)
    {
        _creatingTaskDbOperations = creatingTaskDbOperations;
        _displayCurrentTaskInfo = displayCurrentTaskInfo;
        NextTask = taskDescriptionRequest;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        if (Message.Text.StartsWith("/"))
        {
            await BotClient.SendTextMessageAsync(Message.Chat.Id,
                replyToMessageId: Message.MessageId,
                text: $"Task name should not start with \"/\"\n" +
                      $"Please type a new name for a task");
            return;
        }

        if (IsTemplate)
        {
            await _creatingTaskDbOperations.AddName(task, Message.Text, isTemplate: true);
        }
        else
        {
            await _creatingTaskDbOperations.AddName(task, Message.Text);
        }

        if (IsTemplate)
        {
            NextTask.IsTemplate = true;
            return;
        }
        
        if (task.InEditMode)
        {
            await TaskDbOperations.ToggleEditModeForTask(task);
            NextTask = _displayCurrentTaskInfo;
            NextTask.IsEdit = true;
        }
    }
}