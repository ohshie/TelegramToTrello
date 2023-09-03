using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations;

public abstract class TemplateCreationBaseHandler
{
    protected Message? Message;
    protected CallbackQuery? CallbackQuery;
    protected readonly ITelegramBotClient BotClient;
    
    protected readonly UserDbOperations UserDbOperations;
    protected readonly TemplatesDbOperations TemplateDbOperations;
    private readonly Verifier _verifier;
    
    protected TemplateCreationBaseHandler? NextTask { get; set; }

    protected TemplateCreationBaseHandler(ITelegramBotClient botClient, 
        UserDbOperations userDbOperations, 
        TemplatesDbOperations templateDbOperations, Verifier verifier)
    {
        BotClient = botClient;
        UserDbOperations = userDbOperations;
        TemplateDbOperations = templateDbOperations;
        _verifier = verifier;
    }

    public async Task Execute(Message message)
    {
        Message = message;

        User user = await _verifier.GetUser(message);
        if (user is null) return;

        Template template = await _verifier.GetTemplate(user.TelegramId);
        if (template is null) return;

        await HandleTask(template);
        
        if (NextTask != null)
        {
            await NextTask.Execute(message);
        }
    }

    public async Task Execute(CallbackQuery callbackQuery)
    {
        CallbackQuery = callbackQuery;
        
        User user = await _verifier.GetUser(callbackQuery.Message);
        if (user is null) return;
        
        Template template = await _verifier.GetTemplate(user.TelegramId);
        if (template is null) return;
        
        await HandleTask(template);

        if (NextTask != null)
        {
            await NextTask.Execute(callbackQuery);
        }
    }
    
    protected abstract Task HandleTask(Template template);
}