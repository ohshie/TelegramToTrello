using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public class TaskDescriptionRequest : TaskCreationBaseHandler
{
    public TaskDescriptionRequest(Message message, ITelegramBotClient botClient) : base(message, botClient) {}

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        CreatingTaskDbOperations dbOperations = new(user, task);
        await dbOperations.AddPlaceholderDescription();
        
        await BotClient.SendTextMessageAsync(Message.Chat.Id,
            replyToMessageId: Message.MessageId,
            text: $"Task name successfully set to: {Message.Text}\n" +
                  $"Now please type description of your task in the next message.");
    }
}