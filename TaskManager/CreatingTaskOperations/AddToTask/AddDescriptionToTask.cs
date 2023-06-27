using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

public class AddDescriptionToTask : TaskCreationBaseHandler
{
    public AddDescriptionToTask(Message message, ITelegramBotClient botClient) : base(message, botClient)
    {
        NextTask = new CreateKeyboardWithUsers(Message, BotClient);;
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

        CreatingTaskDbOperations dbOperations = new(user, task);
        await dbOperations.AddDescription(Message.Text);
        
        if (task.InEditMode)
        {
            TaskDbOperations taskDbOperations = new();
            await taskDbOperations.ToggleEditModeForTask(task);
            NextTask = new DisplayCurrentTaskInfo(Message, BotClient);
        }
    }
}