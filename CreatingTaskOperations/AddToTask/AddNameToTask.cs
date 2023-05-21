using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

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
        await dbOperations.SetTaskName(Message.Text);
    }
}