using Telegram.Bot;
using Telegram.Bot.Types;

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

        CreatingTaskDbOperations creatingTaskDbOperations = new(user,null);
        await creatingTaskDbOperations.AddTaskToDb();

        CreateKeyboardWithBoards createKeyboardWithBoards = new(Message, BotClient);
        await createKeyboardWithBoards.Execute();
    }
    
    private async Task<RegisteredUser> GetUser()
    {
        DbOperations dbOperations = new DbOperations();
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
}