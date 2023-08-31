using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class CreateKeyboardWithTables : TaskCreationBaseHandler
{

    public CreateKeyboardWithTables(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations, DbOperations dbOperations, TablesKeyboard tablesKeyboard, Verifier verifier) : base(botClient, userDbOperations, taskDbOperations,verifier)
    {
        _tablesKeyboard = tablesKeyboard;
    }

    private TablesKeyboard _tablesKeyboard;

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        InlineKeyboardMarkup replyKeyboardMarkup;
        
        if (IsEdit)
        {
            replyKeyboardMarkup = await _tablesKeyboard.KeyboardTableChoice(user, task.TrelloBoardId, isEdit: true); 
        }
        else
        {
            replyKeyboardMarkup = await _tablesKeyboard.KeyboardTableChoice(user, task.TrelloBoardId);
        }
        
        await BotClient.EditMessageTextAsync(chatId: CallbackQuery.Message.Chat.Id,
                messageId: CallbackQuery.Message.MessageId,
                text: $"Now choose list on {task.TrelloBoardName}", 
                replyMarkup: replyKeyboardMarkup);
    }
}