using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.CreatingTaskOperations;

public class CreateKeyboardWithBoards : TaskCreationOperator
{
    public CreateKeyboardWithBoards(Message message, ITelegramBotClient botClient) : base(message, botClient) {}

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        InlineKeyboardMarkup inlineKeyboardMarkup = KeyboardBoardChoice(user);
        await BotClient.SendTextMessageAsync(text: "We will start with choosing a board for our task:",
            chatId: Message.Chat.Id,
            replyMarkup: inlineKeyboardMarkup,
            replyToMessageId: Message.MessageId);
    }
    
    private InlineKeyboardMarkup KeyboardBoardChoice(RegisteredUser? user)
    {
        Board?[] boards = user.UsersBoards.Select(ub => ub.Boards).ToArray();

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
                        $"/board {board[i].TrelloBoardId}"),
                    InlineKeyboardButton.WithCallbackData($"{board[i+1].BoardName}",
                        $"/board {board[i+1].TrelloBoardId}")
                });
            }
            else
            {
                keyboardButtonsList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{board[i].BoardName}",
                        $"/board {board[i].TrelloBoardId}")
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
                InlineKeyboardButton.WithCallbackData($"{board.BoardName}",$"/board {board.TrelloBoardId}")
            }); 
        }
        
        return keyboardButtonsList;
    }
}