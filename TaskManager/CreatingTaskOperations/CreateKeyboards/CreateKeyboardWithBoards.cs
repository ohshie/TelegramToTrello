using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;
using TelegramToTrello.Repositories;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class CreateKeyboardWithBoards : TaskCreationBaseHandler
{
    public CreateKeyboardWithBoards(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        BoardsKeyboard boardsKeyboard, Verifier verifier, BotMessenger botMessenger, TaskDbOperations taskDbOperations) : 
        base(botClient, userDbOperations, verifier, botMessenger, taskDbOperations)
    {
        _boardsKeyboard = boardsKeyboard;
    }

    private readonly BoardsKeyboard _boardsKeyboard;

    protected override async Task HandleTask(User user, TTTTask task)
    {
        InlineKeyboardMarkup inlineKeyboardMarkup;
        
        if (IsEdit)
        {
            inlineKeyboardMarkup = _boardsKeyboard.KeyboardBoardChoice(user, IsEdit: true);
            
            await BotMessenger.UpdateMessage(chatId: (int)CallbackQuery.Message.Chat.Id,
                messageId: CallbackQuery.Message.MessageId,
                text: $"Choose new board from list", inlineKeyboardMarkup);
            return;
        }
        
        inlineKeyboardMarkup = _boardsKeyboard.KeyboardBoardChoice(user);
        
        await BotMessenger.SendMessage(text: "We will start with choosing a board for our task:",
            chatId: (int)Message.Chat.Id, 
            replyKeyboardMarkup: inlineKeyboardMarkup);
    }
}