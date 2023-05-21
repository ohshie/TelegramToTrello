using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public abstract class TaskCreationBaseHandler
{
    protected Message? Message { get; }
    protected CallbackQuery? CallbackQuery { get; }
    protected ITelegramBotClient BotClient { get; }

    protected TaskCreationBaseHandler? NextTask { get; set; }
    protected TaskCreationBaseHandler? SubTask { get; set; }

    protected TaskCreationBaseHandler(Message message, ITelegramBotClient botClient)
    {
        Message = message;
        BotClient = botClient;
    }
    
    protected TaskCreationBaseHandler(CallbackQuery callback, ITelegramBotClient botClient)
    {
        CallbackQuery = callback;
        Message = callback.Message;
        BotClient = botClient;
    }
    
    public async Task Execute()
    {
        RegisteredUser user = await GetUser();
        TTTTask task = await GetTask();

        if (!await UserIsRegisteredUser(user)) return;
        if (!await TaskExist(task)) return;

        await HandleTask(user, task);
        
        if (NextTask != null) await NextTask.Execute();
    }

    private async Task<RegisteredUser> GetUser()
    {
        DbOperations dbOperations = new DbOperations();
        RegisteredUser trelloUser = await dbOperations.RetrieveTrelloUser((int)Message.Chat.Id);

        return trelloUser;
    }

    private async Task<TTTTask> GetTask()
    {
        DbOperations dbOperations = new DbOperations();
        TTTTask userTask = await dbOperations.RetrieveUserTask((int)Message.Chat.Id);

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
                text: "Lets not get ahead of ourselves." +
                      "Click on /newtask first to start task creation process");
            return false;
        }
        
        return true;
    }

    protected abstract Task HandleTask(RegisteredUser user, TTTTask task);
}