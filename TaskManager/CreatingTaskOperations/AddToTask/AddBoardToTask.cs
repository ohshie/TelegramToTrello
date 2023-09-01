using Telegram.Bot;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

public class AddBoardToTask : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private readonly CreateKeyboardWithTemplate _createKeyboardWithTemplate;
    private readonly CreateKeyboardWithTables _createKeyboardWithTables;

    public AddBoardToTask(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations,
        CreateKeyboardWithTables createKeyboardWithTables,
        CreatingTaskDbOperations creatingTaskDbOperations, Verifier verifier,
        CreateKeyboardWithTemplate createKeyboardWithTemplate) : base(botClient, userDbOperations, taskDbOperations, verifier)
    {
        _createKeyboardWithTables = createKeyboardWithTables;
        _creatingTaskDbOperations = creatingTaskDbOperations;
        _createKeyboardWithTemplate = createKeyboardWithTemplate;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        string boardId = CheckIfEditForBoardId();
        
        string? boardName = await _creatingTaskDbOperations.AddBoard(task, boardId);
        if (string.IsNullOrEmpty(boardName))
        {
            await BotClient.SendTextMessageAsync(text: "Please choose board name from keyboard menu.",
                chatId: user.TelegramId);
            NextTask = null;
            return;
        }

        NextTask = _createKeyboardWithTemplate;
        
        if (IsEdit)
        {
            NextTask = _createKeyboardWithTables;
            NextTask.IsEdit = true;
        }
    }

    private string CheckIfEditForBoardId()
    {
        string boardId;
        if (IsEdit)
        {
            boardId = CallbackQuery.Data.Substring(CallbackList.TaskEditboard.Length).Trim();
        }
        else
        {
            boardId = CallbackQuery.Data.Substring(CallbackList.Board.Length).Trim();
        }

        return boardId;
    }
}