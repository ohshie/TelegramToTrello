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
        CreateKeyboardWithUsers createKeyboardWithUsers,
        CreatingTaskDbOperations creatingTaskDbOperations,
        DisplayCurrentTaskInfo displayCurrentTaskInfo,
        Verifier verifier, BotMessenger botMessenger, TaskDbOperations taskDbOperations) : 
        base(botClient, userDbOperations, verifier, botMessenger, taskDbOperations)
    {
        _creatingTaskDbOperations = creatingTaskDbOperations;
        _displayCurrentTaskInfo = displayCurrentTaskInfo;
        
        NextTask = createKeyboardWithUsers;
    }

    protected override async Task HandleTask(User user, TTTTask task)
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

        await BotMessenger.RemoveLastBotMessage(user.TelegramId);
        await BotMessenger.RemoveMessage(chatId: user.TelegramId, messageId: Message.MessageId);
        
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