using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;

namespace TelegramToTrello.CreatingTaskOperations;

public abstract class TaskCreationBaseHandler
{
    protected Message? Message;
    protected CallbackQuery? CallbackQuery;
    protected readonly ITelegramBotClient BotClient;
    
    protected readonly UserDbOperations UserDbOperations;
    protected readonly TaskDbOperations TaskDbOperations;
    private readonly Verifier _verifier;

    protected internal bool IsEdit;
    protected internal bool IsTemplate;

    protected TaskCreationBaseHandler? NextTask { get; set; }

    protected TaskCreationBaseHandler(ITelegramBotClient botClient, 
        UserDbOperations dbOperations,
        TaskDbOperations taskDbOperations, Verifier verifier)
    {
        BotClient = botClient;
        UserDbOperations = dbOperations;
        TaskDbOperations = taskDbOperations;
        _verifier = verifier;
    }

    public async Task Execute(Message message, bool isEdit = false, bool isTemplate = false)
    {
        Message = message;
        IsEdit = isEdit;
        IsTemplate = isTemplate;

        RegisteredUser user = await _verifier.GetUser(message);
        if (user is null) return;

        TTTTask task = await _verifier.GetTask(message);
        if (task is null) return;

        await HandleTask(user, task);

        if (NextTask != null)
        {
            if (IsEdit) await NextTask.Execute(message, isEdit: true);
            if (IsTemplate) await NextTask.Execute(message, isTemplate: true);
            else await NextTask.Execute(message);
        }
    }
    
    public async Task Execute(CallbackQuery callback, bool isEdit = false, bool isTemplate = false)
    {
        if (!IsEdit)
        {
            IsEdit = isEdit;
        }

        if (!IsTemplate)
        {
            IsTemplate = isTemplate;
        }
        
        CallbackQuery = callback;
        Message = callback.Message;
        
        RegisteredUser user = await _verifier.GetUser(callback.Message);
        if (user is null) return;

        TTTTask task = await _verifier.GetTask(callback.Message);
        if (task is null) return;

        await HandleTask(user, task);
        
        if (NextTask != null)
        {
            if (isEdit)
            {
                await NextTask.Execute(callback, isEdit: true);
                return;
            }
            if (isTemplate)
            {
                await NextTask.Execute(callback, isTemplate: true);
            }
            
            await NextTask.Execute(callback);
        }
    }

    protected abstract Task HandleTask(RegisteredUser user, TTTTask task);
}