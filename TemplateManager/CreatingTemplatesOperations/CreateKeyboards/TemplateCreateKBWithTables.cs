using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.BotManager;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations.CreateKeyboards;

public class TemplateCreateKBWithTables : TemplateCreationBaseHandler
{
    private readonly TablesKeyboard _tablesKeyboard;
    private readonly BotMessenger _botMessenger;

    public TemplateCreateKBWithTables(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TemplatesDbOperations templateDbOperations, Verifier verifier, TablesKeyboard tablesKeyboard, BotMessenger botMessenger) : base(botClient,
        userDbOperations, templateDbOperations, verifier)
    {
        _tablesKeyboard = tablesKeyboard;
        _botMessenger = botMessenger;
    }

    protected override async Task HandleTask(Template template)
    {
        InlineKeyboardMarkup keyboardMarkup = await _tablesKeyboard.KeyboardTableChoice(template.UserId, template.BoardId, isTemplate: true);
        
        await _botMessenger.UpdateMessage(text: "Next, we need to select a default list:",
            chatId: template.UserId,
            messageId: CallbackQuery.Message.MessageId,
            keyboardMarkup: keyboardMarkup);
    }
}