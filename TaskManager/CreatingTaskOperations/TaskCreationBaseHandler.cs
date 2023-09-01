using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;

namespace TelegramToTrello.CreatingTaskOperations;

public abstract class TaskCreationBaseHandler
{
    protected Message? Message;
    protected CallbackQuery? CallbackQuery;
    protected readonly ITelegramBotClient BotClient;
    private readonly UserDbOperations _userDbOperations;
    protected readonly BotMessenger BotMessenger;
    
    protected readonly TaskDbOperations TaskDbOperations;
    private readonly Verifier _verifier;

    protected internal bool IsEdit;
    protected internal bool IsTemplate;

    protected TaskCreationBaseHandler? NextTask { get; set; }

    protected TaskCreationBaseHandler(ITelegramBotClient botClient,
        UserDbOperations userDbOperations, Verifier verifier, 
        BotMessenger botMessenger, TaskDbOperations taskDbOperations)
    {
        BotClient = botClient;
        _userDbOperations = userDbOperations;
        _verifier = verifier;
        BotMessenger = botMessenger;
        TaskDbOperations = taskDbOperations;
    }

    public async Task Execute(Message message, bool isEdit = false, bool isTemplate = false)
    {
        Message = message;
        IsEdit = isEdit;
        IsTemplate = isTemplate;

        if (!await _verifier.CheckUser((int)message.From.Id)) return;
        if (!await _verifier.CheckTask((int)message.From.Id)) return;
        
        User user = await _userDbOperations.RetrieveTrelloUser((int)message.From.Id);
        TTTTask task = await TaskDbOperations.RetrieveUserTask(user.TelegramId);
        
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
        if (!IsEdit) IsEdit = isEdit;
        if (!IsTemplate) IsTemplate = isTemplate;
        
        CallbackQuery = callback;
        Message = callback.Message;
        
        if (!await _verifier.CheckUser((int)callback.From.Id)) return;
        if (!await _verifier.CheckTask((int)callback.From.Id)) return;
        
        User user = await _userDbOperations.RetrieveTrelloUser((int)callback.From.Id);
        TTTTask task = await TaskDbOperations.RetrieveUserTask((int)callback.From.Id);
        
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

    protected abstract Task HandleTask(User user, TTTTask task);
}