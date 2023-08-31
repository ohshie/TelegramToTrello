using Telegram.Bot;
using TelegramToTrello.BotManager;
using TelegramToTrello.TemplateManager.CreatingTemplatesOperations.CreateKeyboards;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations.AddToTemplate;

public class AddBoardToTemplate : TemplateCreationBaseHandler
{
    private readonly TemplateCreateKBWithTables _createKeyboardWithTables;

    public AddBoardToTemplate(ITelegramBotClient botClient, 
        UserDbOperations userDbOperations, 
        TemplatesDbOperations templatesDbOperations, 
        Verifier verifier,
        TemplateCreateKBWithTables createKeyboardWithTables) : base(botClient, userDbOperations, templatesDbOperations, verifier)
    {
        _createKeyboardWithTables = createKeyboardWithTables;
    }

    protected override async Task HandleTask(RegisteredUser user, Template template)
    {
        string boardId = CallbackQuery.Data
            .Substring(CallbackList.TemplateBoard.Length).Trim();
        
        await TemplateDbOperations.AddBoardToTemplate(template,boardId);
        
        NextTask = _createKeyboardWithTables;
    }
}