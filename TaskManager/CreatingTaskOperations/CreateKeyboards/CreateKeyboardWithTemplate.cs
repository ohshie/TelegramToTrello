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

    public CreateKeyboardWithTemplate(ITelegramBotClient botClient, UserDbOperations userDbOperations, 
        Verifier verifier, 
        TemplatesDbOperations templatesDbOperations,
        CreateKeyboardWithTables createKeyboardWithTables,
        TemplatesKeyboard templatesKeyboard, BotMessenger botMessenger, TaskDbOperations taskDbOperations) : base(botClient, userDbOperations,
        verifier, botMessenger, taskDbOperations)
    {
        _templatesDbOperations = templatesDbOperations;
        _createKeyboardWithTables = createKeyboardWithTables;
        _templatesKeyboard = templatesKeyboard;
    }

    protected override async Task HandleTask(User user, TTTTask task)
    {
        var templates = await _templatesDbOperations.GetAllBoardTemplates(user.TelegramId, task.TrelloBoardId);

        if (templates.Count == null)
        {
            NextTask = _createKeyboardWithTables;
            return;
        }
        
        InlineKeyboardMarkup keyboardMarkup = _templatesKeyboard.KeyboardMarkup(templates);

        await BotMessenger.UpdateMessage(chatId: user.TelegramId, messageId: Message.MessageId,text:
            "You can choose a template if you wish", keyboardMarkup: keyboardMarkup);
    }
}