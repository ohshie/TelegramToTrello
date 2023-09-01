using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class CreateKeyboardWithTemplate : TaskCreationBaseHandler
{
    private readonly TemplatesDbOperations _templatesDbOperations;
    private readonly CreateKeyboardWithTables _createKeyboardWithTables;
    private readonly TemplatesKeyboard _templatesKeyboard;

    public CreateKeyboardWithTemplate(ITelegramBotClient botClient, UserDbOperations dbOperations,
        TaskDbOperations taskDbOperations, Verifier verifier, 
        TemplatesDbOperations templatesDbOperations,
        CreateKeyboardWithTables createKeyboardWithTables,
        TemplatesKeyboard templatesKeyboard) : base(botClient, dbOperations, taskDbOperations,
        verifier)
    {
        _templatesDbOperations = templatesDbOperations;
        _createKeyboardWithTables = createKeyboardWithTables;
        _templatesKeyboard = templatesKeyboard;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        var templates = await _templatesDbOperations.GetAllBoardTemplates(user.TelegramId, task.TrelloBoardId);

        if (templates.Count == null)
        {
            NextTask = _createKeyboardWithTables;
            return;
        }
        
        InlineKeyboardMarkup keyboardMarkup = _templatesKeyboard.KeyboardMarkup(templates);

        await BotClient.SendTextMessageAsync(chatId: user.TelegramId, text:
            "You can choose a template if you wish", replyMarkup: keyboardMarkup);
    }
}