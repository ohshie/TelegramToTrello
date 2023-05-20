using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public class AddTableToTask : TaskCreationOperator
{
    public AddTableToTask(CallbackQuery callback, ITelegramBotClient botClient) : base(callback, botClient)
    {
        NextTask = new CreateKeyboardWithTags(callback,botClient);
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        string listName = CallbackQuery.Data.Substring("/list".Length).Trim();

        CreatingTaskDbOperations dbOperations = new(user, task);
        bool listExist = await dbOperations.AddTableToTask(listName);
        if (!listExist)
        {
            await BotClient.SendTextMessageAsync(text: "Please choose list name from keyboard menu.",
                chatId: Message.Chat.Id,
                replyToMessageId: Message.MessageId);
            NextTask = null;
        }
    }
}