using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public abstract class TaskCreationOperator
{
    protected Message? Message { get; }
    protected CallbackQuery? CallbackQuery { get; }
    protected ITelegramBotClient BotClient { get; }

    protected RegisteredUser? User { get; set; }
    protected TTTTask? UserTask { get; set; }
    
    protected TaskCreationOperator? NextTask { get; set; }
    protected TaskCreationOperator? SubTask { get; set; }

    protected TaskCreationOperator(Message message, ITelegramBotClient botClient)
    {
        Message = message;
        BotClient = botClient;
    }
    
    protected TaskCreationOperator(CallbackQuery callback, ITelegramBotClient botClient)
    {
        CallbackQuery = callback;
        Message = callback.Message;
        BotClient = botClient;
    }
    
    public async Task Execute()
    {
        User ??= await GetUser();
        UserTask ??= await GetTask();

        if (!await UserIsRegisteredUser(User)) return;
        if (!await TaskExist(UserTask)) return;

       await HandleTask(User, UserTask);

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