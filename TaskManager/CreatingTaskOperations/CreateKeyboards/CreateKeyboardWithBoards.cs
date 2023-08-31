using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class CreateKeyboardWithBoards : TaskCreationBaseHandler
{
    public CreateKeyboardWithBoards(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations, BoardsKeyboard boardsKeyboard, Verifier verifier) : base(botClient, userDbOperations, taskDbOperations, verifier)
    {
        _boardsKeyboard = boardsKeyboard;
    }

    private readonly BoardsKeyboard _boardsKeyboard;
    
    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        InlineKeyboardMarkup inlineKeyboardMarkup;
        
        if (IsEdit)
        {
            inlineKeyboardMarkup = _boardsKeyboard.KeyboardBoardChoice(user, IsEdit: true);
            
            await BotClient.EditMessageTextAsync(chatId: CallbackQuery.Message.Chat.Id,
                messageId: CallbackQuery.Message.MessageId,
                text: $"Choose new board from list", replyMarkup: inlineKeyboardMarkup);
            return;
        }
        
        inlineKeyboardMarkup = _boardsKeyboard.KeyboardBoardChoice(user);
        
        await BotClient.SendTextMessageAsync(text: "We will start with choosing a board for our task:",
            chatId: Message.Chat.Id,
            replyMarkup: inlineKeyboardMarkup);
    }
}