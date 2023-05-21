using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public class AddDescriptionToTask : TaskCreationBaseHandler
{
    public AddDescriptionToTask(Message message, ITelegramBotClient botClient) : base(message, botClient)
    {
        NextTask = null;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        if (Message.Text.StartsWith("/"))
        {
            await BotClient.SendTextMessageAsync(Message.Chat.Id,
                replyToMessageId: Message.MessageId,
                text: $"Task description should not start with \"/\"\n" +
                      $"Please type a new description for a task");
            return;
        }

        CreatingTaskDbOperations dbOperations = new(user, task);
        await dbOperations.SetDescription(Message.Text);
        NextTask = new CreateKeyboardWithUsers(Message, BotClient);
    }
}