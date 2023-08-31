using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.TaskManager.CreatingTaskOperations;

namespace TelegramToTrello.CreatingTaskOperations;

public class StartTaskCreation
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private Message? _message;
    private readonly CreateKeyboardWithBoards _createKeyboardWithBoards;
    private readonly MessageRemover _messageRemover;
    private readonly Verifier _verifier;

    private ITelegramBotClient BotClient { get; }
    
    public StartTaskCreation(ITelegramBotClient botClient, 
        CreatingTaskDbOperations creatingTaskDbOperations, 
        CreateKeyboardWithBoards createKeyboardWithBoards,
        MessageRemover messageRemover,
        Verifier verifier)
    {
        _creatingTaskDbOperations = creatingTaskDbOperations;
        _createKeyboardWithBoards = createKeyboardWithBoards;
        _messageRemover = messageRemover;
        _verifier = verifier;
        BotClient = botClient;
    }
    
    public async Task CreateTask(Message message)
    {
        _message = message;
        
        var user = await _verifier.GetUser(message);
        if (user is null)
        {
            await _messageRemover.Remove(_message.Chat.Id, _message.MessageId);
            return;
        }

        TTTTask task = await _verifier.GetTask(message);
        if (task is not null)
        {
            await _messageRemover.Remove(_message.Chat.Id, _message.MessageId);
            await BotClient.SendTextMessageAsync(chatId: _message.From.Id,
                text: "Looks like you are already in the process of creating a task.\n" +
                      "Please finish it first or drop it by pressing cancel task");
            return;
        }
        
        await _creatingTaskDbOperations.CreateTask(user);

        await _messageRemover.Remove(_message.Chat.Id, _message.MessageId);
        
        await _createKeyboardWithBoards.Execute(_message);
    }
}