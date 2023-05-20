using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public class AddBoardToTask : TaskCreationOperator
{
    public AddBoardToTask(CallbackQuery callback, ITelegramBotClient botClient) : base(callback, botClient)
    {
        NextTask = new CreateKeyboardWithTables(callback, botClient);
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        string boardId = CallbackQuery.Data.Substring("/board".Length).Trim();
        Console.WriteLine(boardId);

        CreatingTaskDbOperations dbOperations = new(user,task);
        string? boardName = await dbOperations.AddBoardToTask(boardId);
        if (string.IsNullOrEmpty(boardName))
        {
            await BotClient.SendTextMessageAsync(text: "Please choose board name from keyboard menu.",
                chatId: Message.Chat.Id,
                replyToMessageId: Message.MessageId);
            NextTask = null;
        }
    }
}