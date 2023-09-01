using Telegram.Bot;
using TelegramToTrello.BotManager;
using TelegramToTrello.TemplateManager.CreatingTemplatesOperations.RequestFromUser;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations.AddToTemplate;

public class AddTableToTemplate : TemplateCreationBaseHandler
{
    private readonly RequestName _requestName;

    public AddTableToTemplate(ITelegramBotClient botClient, 
        UserDbOperations userDbOperations, 
        TemplatesDbOperations templatesDbOperations, 
        Verifier verifier, RequestName requestName) : base(botClient, userDbOperations, templatesDbOperations, verifier)
    {
        _requestName = requestName;
    }

    protected override async Task HandleTask(User user, Template template)
    {
        string tableName = CallbackQuery.Data
            .Substring(CallbackList.TemplateList.Length).Trim();
        
        await TemplateDbOperations.AddTableToTemplate(template, tableName);

        NextTask = _requestName;
    }
}