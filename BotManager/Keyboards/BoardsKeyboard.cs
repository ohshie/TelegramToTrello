using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.BotManager;

public class BoardsKeyboard
{
    private string? _boardType;
    private readonly UserDbOperations _userDbOperations;

    public BoardsKeyboard(UserDbOperations userDbOperations)
    {
        _userDbOperations = userDbOperations;
    }

    public async Task<InlineKeyboardMarkup> KeyboardBoardChoice(int telegramId, 
        bool IsEdit = false, 
        bool isTemplate = false)
    {
        var user = await _userDbOperations.RetrieveTrelloUser(telegramId);
        Board?[] boards = user.Boards.ToArray();

        if (IsEdit)
        {
            _boardType = "/editboard ";
        }
        else if (isTemplate)
        {
            _boardType = "/templateboard ";
        }
        else
        {
            _boardType = "/board ";
        }
        
        return BoardKeyboard(boards);
    }

    private InlineKeyboardMarkup BoardKeyboard(Board?[] boards)
    {
        if (boards.Length > 8)
        {
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(TwoRowKeyboard(boards));
            return inlineKeyboard;
        }
        else
        {
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(SingleRowKeyboard(boards));
            return inlineKeyboard;
        }
    }

    private List<InlineKeyboardButton[]> TwoRowKeyboard(Board[] board)
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();
        
        for (int i = 0; i < board.Length; i +=2)
        {
            if (i < board.Length-1)
            {
                keyboardButtonsList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{board[i].BoardName}",
                        _boardType+board[i].TrelloBoardId),
                    InlineKeyboardButton.WithCallbackData($"{board[i + 1].BoardName}",
                        _boardType+board[i + 1].TrelloBoardId)
                });
            }
            else
            {
                keyboardButtonsList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{board[i].BoardName}",
                        _boardType+board[i].TrelloBoardId)
                });
            }
        }
        return keyboardButtonsList;
    }
    
    private List<InlineKeyboardButton[]> SingleRowKeyboard(Board[] boards)
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();

        foreach (var board in boards)
        {
            keyboardButtonsList.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData($"{board.BoardName}",_boardType+board.TrelloBoardId)
            }); 
        }
        
        return keyboardButtonsList;
    }
}