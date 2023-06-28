using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class CreateKeyboardWithTables : TaskCreationBaseHandler
{
    private readonly DbOperations _dbOperations;

    public CreateKeyboardWithTables(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations, DbOperations dbOperations) : base(botClient, userDbOperations, taskDbOperations)
    {
        _dbOperations = dbOperations;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        InlineKeyboardMarkup replyKeyboardMarkup = await KeyboardTableChoice(user, task); 
        
        await BotClient.EditMessageTextAsync(chatId: CallbackQuery.Message.Chat.Id,
                messageId: CallbackQuery.Message.MessageId,
                text: $"Now choose list on {task.TrelloBoardName}", 
                replyMarkup: replyKeyboardMarkup);
    }
    
    private async Task<InlineKeyboardMarkup> KeyboardTableChoice(RegisteredUser user, TTTTask task)
    {
        Board selectedBoard = await _dbOperations.RetrieveBoard(user.TelegramId, task.TrelloBoardId);

        if (IsEdit)
        {
            return TablesKeyboardForEdit(selectedBoard);
        }
        
        return TablesKeyboardNormal(selectedBoard);
    }

    private InlineKeyboardMarkup TablesKeyboardNormal(Board selectedBoard)
    {
        if (selectedBoard.Tables.Count > 8)
        {
            InlineKeyboardMarkup replyKeyboardMarkup = new InlineKeyboardMarkup(TwoRowKeyboard(selectedBoard));
            return replyKeyboardMarkup;
        }
        else
        {
            InlineKeyboardMarkup replyKeyboardMarkup = new InlineKeyboardMarkup(SingleRowKeyboard(selectedBoard));
            return replyKeyboardMarkup;
        }
    }

    private InlineKeyboardMarkup TablesKeyboardForEdit(Board selectedBoard)
    {
        if (selectedBoard.Tables.Count > 8)
        {
            InlineKeyboardMarkup replyKeyboardMarkup = new InlineKeyboardMarkup(TwoRowKeyboardEdit(selectedBoard));
            return replyKeyboardMarkup;
        }
        else
        {
            InlineKeyboardMarkup replyKeyboardMarkup = new InlineKeyboardMarkup(SingleRowKeyboardEdit(selectedBoard));
            return replyKeyboardMarkup;
        }
    }

    private List<InlineKeyboardButton[]> TwoRowKeyboard(Board board)
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();

        var tables = board.Tables!.ToArray();
        for (int i = 0; i < tables.Length; i +=2)
        {
            if (i < tables.Length-1)
            {
                keyboardButtonsList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{tables[i]}",
                        $"/list {tables[i]}"),
                    InlineKeyboardButton.WithCallbackData($"{tables[i+1]}",
                        $"/list {tables[i+1]}")
                });
            }
            else
            {
                keyboardButtonsList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{tables[i]}",
                        $"/list {tables[i]}")
                });
            }
        }
        return keyboardButtonsList;
    }

    private List<InlineKeyboardButton[]> SingleRowKeyboard(Board board)
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();

        foreach (var table in board.Tables!)
        {
            keyboardButtonsList.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData($"{table.Name}",$"/list {table.Name}")
            }); 
        }
        
        return keyboardButtonsList;
    }
    
    private List<InlineKeyboardButton[]> TwoRowKeyboardEdit(Board board)
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();

        var tables = board.Tables!.ToArray();
        for (int i = 0; i < tables.Length; i +=2)
        {
            if (i < tables.Length-1)
            {
                keyboardButtonsList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{tables[i]}",
                        $"/editlist {tables[i]}"),
                    InlineKeyboardButton.WithCallbackData($"{tables[i+1]}",
                        $"/editlist {tables[i+1]}")
                });
            }
            else
            {
                keyboardButtonsList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{tables[i]}",
                        $"/editlist {tables[i]}")
                });
            }
        }
        return keyboardButtonsList;
    }

    private List<InlineKeyboardButton[]> SingleRowKeyboardEdit(Board board)
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();

        foreach (var table in board.Tables!)
        {
            keyboardButtonsList.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData($"{table.Name}",$"/editlist {table.Name}")
            }); 
        }
        
        return keyboardButtonsList;
    }
}