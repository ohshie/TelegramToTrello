using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

public class AddNameToTask : TaskCreationBaseHandler
{
    public AddNameToTask(Message message, ITelegramBotClient botClient) : base(message, botClient)
    {
        NextTask = new TaskDescriptionRequest(message, botClient);
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

        CreatingTaskDbOperations dbOperations = new(user, task);
        await dbOperations.AddName(Message.Text);
        
        if (task.InEditMode)
        {
            TaskDbOperations taskDbOperations = new();
            await taskDbOperations.ToggleEditModeForTask(task);
            NextTask = new DisplayCurrentTaskInfo(Message, BotClient);
        }
    }
}