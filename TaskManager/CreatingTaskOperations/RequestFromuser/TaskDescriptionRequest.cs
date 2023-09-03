using Telegram.Bot;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.RequestFromuser;

public class TaskDescriptionRequest : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;

    public TaskDescriptionRequest(ITelegramBotClient botClient, 
        UserDbOperations userDbOperations, 
        CreatingTaskDbOperations creatingTaskDbOperations, Verifier verifier, 
        BotMessenger botMessenger, TaskDbOperations taskDbOperations) : 
        base(botClient, userDbOperations, verifier, botMessenger, taskDbOperations)
    {
        _creatingTaskDbOperations = creatingTaskDbOperations;
    }
    

    protected override async Task HandleTask(TTTTask task)
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
            await TaskDbOperations.ToggleEditModeForTask(task);
            await BotMessenger.SendMessage(text: "Now please type name of your task in the next message.",
                chatId: task.Id);
            return;
        }
        
        await BotMessenger.SendMessage(chatId: task.Id,
            text: $"Task name successfully set to: {Message.Text}\n" +
                  $"Now please type description of your task in the next message.");
    }
}