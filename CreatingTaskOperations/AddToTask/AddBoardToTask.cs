using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public class AddBoardToTask : TaskCreationBaseHandler
{
    private bool IsEdit { get; set; }
    
    public AddBoardToTask(CallbackQuery callback, ITelegramBotClient botClient, bool isEdit = false) : base(callback, botClient)
    {
        NextTask = new CreateKeyboardWithTables(callback, botClient);
        IsEdit = isEdit;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        string boardId = CheckIfEditForBoardId();
        
        CreatingTaskDbOperations dbOperations = new(user,task);
        string? boardName = await dbOperations.AddBoard(boardId);
        if (string.IsNullOrEmpty(boardName))
        {
            await BotClient.SendTextMessageAsync(text: "Please choose board name from keyboard menu.",
                chatId: Message.Chat.Id,
                replyToMessageId: Message.MessageId);
            NextTask = null;
            return;
        }

        if (IsEdit)
        {
            NextTask = new CreateKeyboardWithTables(CallbackQuery, BotClient, true);
        }
    }

    private string CheckIfEditForBoardId()
    {
        string boardId;
        if (IsEdit)
        {
            boardId = CallbackQuery.Data.Substring("/editboard".Length).Trim();
        }
        else
        {
            boardId = CallbackQuery.Data.Substring("/board".Length).Trim();
        }

        return boardId;
    }
}