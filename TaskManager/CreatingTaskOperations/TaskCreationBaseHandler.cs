using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.UserRegistration;

namespace TelegramToTrello.CreatingTaskOperations;

public abstract class TaskCreationBaseHandler
{
    protected Message? Message;
    protected CallbackQuery? CallbackQuery;
    protected readonly ITelegramBotClient BotClient;
    
    protected readonly UserDbOperations UserDbOperations;
    protected readonly TaskDbOperations TaskDbOperations;

    protected internal bool IsEdit;

    protected TaskCreationBaseHandler? NextTask { get; set; }
    protected TaskCreationBaseHandler? SubTask { get; set; }

    protected TaskCreationBaseHandler(ITelegramBotClient botClient, 
        UserDbOperations dbOperations,
        TaskDbOperations taskDbOperations)
    {
        BotClient = botClient;
        UserDbOperations = dbOperations;
        TaskDbOperations = taskDbOperations;
    }

    public async Task Execute(Message message, bool isEdit = false)
    {
        Message = message;
        IsEdit = isEdit;

        RegisteredUser user = await GetUser();
        TTTTask task = await GetTask();

        if (!await UserIsRegisteredUser(user)) return;
        if (!await TaskExist(task)) return;

        await HandleTask(user, task);
        
        if (NextTask != null) await NextTask.Execute(message);
    }
    
    public async Task Execute(CallbackQuery callback, bool isEdit = false)
    {
        CallbackQuery = callback;
        Message = CallbackQuery.Message;
        IsEdit = isEdit;

        RegisteredUser user = await GetUser();
        TTTTask task = await GetTask();

        if (!await UserIsRegisteredUser(user)) return;
        if (!await TaskExist(task)) return;

        await HandleTask(user, task);
        
        if (NextTask != null) await NextTask.Execute(callback);
    }

    private async Task<RegisteredUser> GetUser()
    {
        RegisteredUser trelloUser = await UserDbOperations.RetrieveTrelloUser((int)Message.Chat.Id);

        return trelloUser;
    }

    private async Task<TTTTask> GetTask()
    {
        TTTTask userTask = await TaskDbOperations.RetrieveUserTask((int)Message.Chat.Id);

        return userTask;
    }

    private async Task<bool> UserIsRegisteredUser(RegisteredUser? user)
    {
        if (user == null)
        {
            await BotClient.SendTextMessageAsync(chatId: Message.From.Id,
                text: "Looks like you are not registered yet." +
                      "Click on /register and follow commands to register");
            return false;
        }

        return true;
    }

    private async Task<bool> TaskExist(TTTTask? task)
    {
        if (task == null)
        {
            await BotClient.SendTextMessageAsync(chatId: Message.From.Id,
                text: "Lets not get ahead of ourselves.\n" +
                      "Click on /newtask first to start task creation process");
            return false;
        }
        
        return true;
    }

    protected abstract Task HandleTask(RegisteredUser user, TTTTask task);
}