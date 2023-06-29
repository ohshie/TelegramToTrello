using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

public class AddBoardToTask : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private CreateKeyboardWithTables _createKeyboardWithTables;

    public AddBoardToTask(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations,
        CreateKeyboardWithTables createKeyboardWithTables,
        CreatingTaskDbOperations creatingTaskDbOperations) : base(botClient, userDbOperations, taskDbOperations)
    {
        _createKeyboardWithTables = createKeyboardWithTables;
        _creatingTaskDbOperations = creatingTaskDbOperations;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        string boardId = CheckIfEditForBoardId();
        
        string? boardName = await _creatingTaskDbOperations.AddBoard(task, boardId);
        if (string.IsNullOrEmpty(boardName))
        {
            await BotClient.SendTextMessageAsync(text: "Please choose board name from keyboard menu.",
                chatId: Message.Chat.Id,
                replyToMessageId: Message.MessageId);
            NextTask = null;
            return;
        }

        NextTask = _createKeyboardWithTables;
        
        if (IsEdit)
        {
            NextTask.IsEdit = true;
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