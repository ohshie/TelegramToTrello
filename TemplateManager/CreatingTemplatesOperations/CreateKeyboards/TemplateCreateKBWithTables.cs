using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.BotManager;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations.CreateKeyboards;

public class TemplateCreateKBWithTables : TemplateCreationBaseHandler
{
    private readonly TablesKeyboard _tablesKeyboard;

    public TemplateCreateKBWithTables(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TemplatesDbOperations templateDbOperations, Verifier verifier, TablesKeyboard tablesKeyboard) : base(botClient,
        userDbOperations, templateDbOperations, verifier)
    {
        _tablesKeyboard = tablesKeyboard;
    }

    protected override async Task HandleTask(User user, Template template)
    {
        InlineKeyboardMarkup keyboardMarkup = await _tablesKeyboard.KeyboardTableChoice(user, template.BoardId, isTemplate: true);
        
        await BotClient.SendTextMessageAsync(text: "Next, we need to select a default list:",
            chatId: user.TelegramId,
            replyMarkup: keyboardMarkup);
    }
}