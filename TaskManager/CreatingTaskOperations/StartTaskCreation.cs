using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.TaskManager.CreatingTaskOperations;

namespace TelegramToTrello.CreatingTaskOperations;

public class StartTaskCreation
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private readonly CreateKeyboardWithBoards _createKeyboardWithBoards;
    private readonly BotMessenger _botMessenger;
    private readonly Verifier _verifier;

    private ITelegramBotClient BotClient { get; }
    
    public StartTaskCreation(ITelegramBotClient botClient, 
        CreatingTaskDbOperations creatingTaskDbOperations, 
        CreateKeyboardWithBoards createKeyboardWithBoards,
        BotMessenger botMessenger,
        Verifier verifier)
    {
        _creatingTaskDbOperations = creatingTaskDbOperations;
        _createKeyboardWithBoards = createKeyboardWithBoards;
        _botMessenger = botMessenger;
        _verifier = verifier;
        BotClient = botClient;
    }
    
    public async Task CreateTask(Message message)
    {
        await _botMessenger.RemoveLastBotMessage((int)(message.From.Id));
        
        var chatId = (int)message.Chat.Id;
        var messageId = message.MessageId;
        
        await _botMessenger.RemoveMessage(chatId, messageId);
        
        var userExist = await _verifier.CheckUser(chatId);
        if (!userExist)
        {
            return;
        }

        var taskExist = await _verifier.CheckTask(chatId);
        if (taskExist)
        {
            await _botMessenger.SendMessage(chatId: chatId,
                text: "Looks like you are already in the process of creating a task.\n" +
                      "Please finish it first or drop it by pressing cancel task");
            return;
        }
        
        await _creatingTaskDbOperations.CreateTask(chatId);
        await _createKeyboardWithBoards.Execute(message);
    }
}