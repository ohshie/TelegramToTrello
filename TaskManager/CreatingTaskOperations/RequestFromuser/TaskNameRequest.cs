using Telegram.Bot;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.RequestFromuser;

public class TaskNameRequest : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;

    public TaskNameRequest(ITelegramBotClient botClient, 
        UserDbOperations userDbOperations, 
        CreatingTaskDbOperations creatingTaskDbOperations, Verifier verifier, 
        BotMessenger botMessenger, TaskDbOperations taskDbOperations) : 
        base(botClient, userDbOperations, verifier, botMessenger, taskDbOperations)
    {
        _creatingTaskDbOperations = creatingTaskDbOperations;
    }

    protected override async Task HandleTask(User user, TTTTask task)
    {
        if (IsTemplate)
        {
            await _creatingTaskDbOperations.AddPlaceholderName(task, isTemplate: true);
        }
        else
        {
            await _creatingTaskDbOperations.AddPlaceholderName(task);
        }
        
        await SendRequestToUser(task, user);
    }

    private async Task SendRequestToUser(TTTTask task, User user)
    {
        if (IsEdit)
        {
            await TaskDbOperations.ToggleEditModeForTask(task);
        }
        
        await BotMessenger.RemoveMessage(chatId: user.TelegramId, CallbackQuery.Message.MessageId);

        if (IsTemplate)
        {
            string messageText = $"Add something to {task.TaskName
                .Substring(0, task.TaskName.Length - "##template##".Length).Trim()}";
            
            await BotMessenger.SendMessage(text: messageText,
                chatId: user.TelegramId);
            return;
        }
        
        await BotMessenger.SendMessage(text: "Now please type name of your task in the next message.",
            chatId: user.TelegramId);
    }
}