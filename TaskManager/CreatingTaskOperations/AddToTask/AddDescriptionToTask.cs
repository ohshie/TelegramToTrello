using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

public class AddDescriptionToTask : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private readonly DisplayCurrentTaskInfo _displayCurrentTaskInfo;

    public AddDescriptionToTask(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations,
        CreateKeyboardWithUsers createKeyboardWithUsers,
        CreatingTaskDbOperations creatingTaskDbOperations,
        DisplayCurrentTaskInfo displayCurrentTaskInfo,
        Verifier verifier) : base(botClient, userDbOperations, taskDbOperations, verifier)
    {
        _creatingTaskDbOperations = creatingTaskDbOperations;
        _displayCurrentTaskInfo = displayCurrentTaskInfo;
        
        NextTask = createKeyboardWithUsers;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        if (Message.Text.StartsWith("/"))
        {
            await BotClient.SendTextMessageAsync(Message.Chat.Id,
                replyToMessageId: Message.MessageId,
                text: $"Task description should not start with \"/\"\n" +
                      $"Please type a new description for a task");
            NextTask = null;
            return;
        }

        if (IsTemplate)
        {
            await _creatingTaskDbOperations.AddDescription(task,Message.Text, isTemplate: true);
            NextTask.IsTemplate = true;
        }
        else
        {
            await _creatingTaskDbOperations.AddDescription(task,Message.Text);
        }
        
        if (task.InEditMode)
        {
            await TaskDbOperations.ToggleEditModeForTask(task);
            NextTask = _displayCurrentTaskInfo;
        }
    }
}