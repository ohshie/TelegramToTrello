using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.TaskManager.CreatingTaskOperations;

namespace TelegramToTrello.CreatingTaskOperations;

public class StartTaskCreation
{
    private readonly UserDbOperations _userDbOperations;
    private readonly TaskDbOperations _taskDbOperations;
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private Message? _message;
    private CreateKeyboardWithBoards _createKeyboardWithBoards;

    private ITelegramBotClient BotClient { get; }


    public StartTaskCreation(ITelegramBotClient botClient, 
        UserDbOperations userDbOperations, 
        TaskDbOperations taskDbOperations,
        CreatingTaskDbOperations creatingTaskDbOperations, 
        CreateKeyboardWithBoards createKeyboardWithBoards)
    {
        _userDbOperations = userDbOperations;
        _taskDbOperations = taskDbOperations;
        _creatingTaskDbOperations = creatingTaskDbOperations;
        _createKeyboardWithBoards = createKeyboardWithBoards;
        BotClient = botClient;
    }
    
    public async Task CreateTask(Message message)
    {
        _message = message;
        
        var user = await GetUser();
        
        if (!await UserIsRegisteredUser(user)) return;
        if (await UserIsCreatingATask(user)) return;
        
        await _creatingTaskDbOperations.CreateTask(user);

        await BotClient.DeleteMessageAsync(chatId: _message.Chat.Id, _message.MessageId);
        
        await _createKeyboardWithBoards.Execute(_message);
    }
    
    private async Task<RegisteredUser?> GetUser()
    {
        RegisteredUser? trelloUser = await _userDbOperations.RetrieveTrelloUser((int)_message.Chat.Id);

        return trelloUser;
    }
    
    private async Task<bool> UserIsRegisteredUser(RegisteredUser? user)
    {
        if (user == null)
        {
            await BotClient.DeleteMessageAsync(chatId: _message.Chat.Id, _message.MessageId);
            await BotClient.SendTextMessageAsync(chatId: _message.From.Id,
                text: "Looks like you are not registered yet." +
                      "Click on /register and follow commands to register");
            return false;
        }

        return true;
    }

    private async Task<bool> UserIsCreatingATask(RegisteredUser user)
    {
        var task = await _taskDbOperations.RetrieveUserTask(user.TelegramId);

        if (task != null)
        {
            await BotClient.DeleteMessageAsync(chatId: _message.Chat.Id, _message.MessageId);
            await BotClient.SendTextMessageAsync(chatId: _message.From.Id,
                text: "Looks like you are already in the process of creating a task.\n" +
                      "Please finish it first or drop it by pressing /drop");
            return true;
        }

        return false;
    }
}