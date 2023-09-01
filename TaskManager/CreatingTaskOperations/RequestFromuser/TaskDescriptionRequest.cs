using Telegram.Bot;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.RequestFromuser;

public class TaskDescriptionRequest : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private readonly MessageRemover _messageRemover;

    public TaskDescriptionRequest(ITelegramBotClient botClient, 
        UserDbOperations dbOperations, 
        TaskDbOperations taskDbOperations,
        CreatingTaskDbOperations creatingTaskDbOperations, Verifier verifier, MessageRemover messageRemover) : base(botClient, dbOperations, taskDbOperations, verifier)
    {
        _creatingTaskDbOperations = creatingTaskDbOperations;
        _messageRemover = messageRemover;
    }
    

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        if (IsTemplate)
        {
            await _creatingTaskDbOperations.AddPlaceholderDescription(task, isTemplate: true);
        }
        else
        {
            await _creatingTaskDbOperations.AddPlaceholderDescription(task);
        }
        
        if (IsEdit)
        {
            await SendRequestToUser(task);
            return;
        }
        
        await BotClient.SendTextMessageAsync(Message.Chat.Id,
            replyToMessageId: Message.MessageId,
            text: $"Task name successfully set to: {Message.Text}\n" +
                  $"Now please type description of your task in the next message.");
    }
    
    private async Task SendRequestToUser(TTTTask task)
    {
        await TaskDbOperations.ToggleEditModeForTask(task);
        
        await _messageRemover.Remove(CallbackQuery.Message.Chat.Id, CallbackQuery.Message.MessageId);
       
        await BotClient.SendTextMessageAsync(text: "Now please type name of your task in the next message.",
            chatId: Message.Chat.Id);
    }
}