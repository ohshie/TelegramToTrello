using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class CreateKeyboardWithTables : TaskCreationBaseHandler
{
    public CreateKeyboardWithTables(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TablesKeyboard tablesKeyboard, Verifier verifier, BotMessenger botMessenger, TaskDbOperations taskDbOperations) : 
        base(botClient, userDbOperations, verifier, botMessenger, taskDbOperations)
    {
        _tablesKeyboard = tablesKeyboard;
    }

    private readonly TablesKeyboard _tablesKeyboard;

    protected override async Task HandleTask(TTTTask task)
    {
        InlineKeyboardMarkup replyKeyboardMarkup = IsEdit
            ? await _tablesKeyboard.KeyboardTableChoice(task.Id, task.TrelloBoardId, isEdit: true)
                                        : await _tablesKeyboard.KeyboardTableChoice(task.Id, task.TrelloBoardId);
        
        await BotMessenger.UpdateMessage(chatId: task.Id,
                messageId: CallbackQuery.Message.MessageId,
                text: $"Now choose list on {task.TrelloBoardName}", 
                keyboardMarkup: replyKeyboardMarkup);
    }
}