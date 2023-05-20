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
    
    private InlineKeyboardMarkup KeyboardBoardChoice(RegisteredUser user)
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();
        
        foreach (var board in user.UsersBoards.Select(ub => ub.Boards))
        {
            keyboardButtonsList.Add(new InlineKeyboardButton[]
                {  InlineKeyboardButton.WithCallbackData($"{board.BoardName}",$"/board {board.TrelloBoardId}") });
        }

        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(keyboardButtonsList);

        return inlineKeyboard;
    }
}