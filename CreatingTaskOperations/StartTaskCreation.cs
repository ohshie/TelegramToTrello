using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.UserRegistration;

namespace TelegramToTrello.CreatingTaskOperations;

public class StartTaskCreation
{
    private ITelegramBotClient BotClient { get; }
    private Message Message { get; }
    
    public StartTaskCreation(Message message, ITelegramBotClient botClient)
    {
        Message = message;
        BotClient = botClient;
    }
    
    public async Task CreateTask()
    {
        var user = await GetUser();

        if (!await UserIsRegisteredUser(user)) return;
        if (await UserIsCreatingATask(user)) return;

        CreatingTaskDbOperations creatingTaskDbOperations = new(user,null);
        await creatingTaskDbOperations.CreateTask();

        CreateKeyboardWithBoards createKeyboardWithBoards = new(Message, BotClient);
        await createKeyboardWithBoards.Execute();
    }
    
    private async Task<RegisteredUser> GetUser()
    {
        UserDbOperations dbOperations = new();
        RegisteredUser trelloUser = await dbOperations.RetrieveTrelloUser((int)Message.Chat.Id);

        return trelloUser;
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

    private async Task<bool> UserIsCreatingATask(RegisteredUser user)
    {
        TaskDbOperations dbOperations = new();
        var task = await dbOperations.RetrieveUserTask(user.TelegramId);

        if (task != null)
        {
            await BotClient.SendTextMessageAsync(chatId: Message.From.Id,
                text: "Looks like you are already in the process of creating a task.\n" +
                      "Please finish it first or drop it by pressing /drop");
            return true;
        }

        return false;
    }
}