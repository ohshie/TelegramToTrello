using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.BotManager;

public class TablesKeyboard
{
    private readonly DbOperations _dbOperations;

    public TablesKeyboard(DbOperations dbOperations)
    {
        _dbOperations = dbOperations;
    }

    private string? _boardType;
    
    public async Task<InlineKeyboardMarkup> KeyboardTableChoice(RegisteredUser user, 
        string boardId, 
        bool isEdit = false, 
        bool isTemplate = false)
    {
        Board selectedBoard = await _dbOperations.RetrieveBoard(user.TelegramId, boardId);

        if (isEdit)
        {
            _boardType = "/editlist ";
        }
        else if (isTemplate)
        {
            _boardType = "/templatelist ";
        }
        else
        {
            _boardType = "/list ";
        }
        
        return AssembleKeyboard(selectedBoard);
    }

    private InlineKeyboardMarkup AssembleKeyboard(Board selectedBoard)
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
                         _boardType+tables[i]),
                    InlineKeyboardButton.WithCallbackData($"{tables[i+1]}",
                        _boardType+tables[i])
                });
            }
            else
            {
                keyboardButtonsList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{tables[i]}",
                        _boardType+tables[i])
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
                InlineKeyboardButton.WithCallbackData($"{table.Name}",_boardType+table.Name)
            }); 
        }
        
        return keyboardButtonsList;
    }
}