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

    protected override async Task HandleTask(TTTTask task)
    {
        if (Message.Text.StartsWith("/"))
        {
            await BotMessenger.SendMessage(chatId: task.Id,
                text: $"Task description should not start with \"/\"\n" +
                      $"Please type a new description for a task");
            NextTask = null;
            return;
        }

        await BotMessenger.RemoveLastBotMessage(task.Id);
        await BotMessenger.RemoveMessage(chatId: task.Id, messageId: Message.MessageId);
        
        if (task.TaskDesc.Contains("##template##"))
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