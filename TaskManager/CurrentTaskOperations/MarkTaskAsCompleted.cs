using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.ToFromTrello;

namespace TelegramToTrello.TaskManager.CurrentTaskOperations;

public class MarkTaskAsCompleted
{
    private readonly ITelegramBotClient _botClient;
    private readonly TaskDbOperations _taskDbOperations;
    private readonly UserDbOperations _userDbOperations ;
    
    public MarkTaskAsCompleted(ITelegramBotClient botClient, 
        TaskDbOperations taskDbOperations, 
        UserDbOperations userDbOperations)
    {
        _botClient = botClient;
        _taskDbOperations = taskDbOperations;
        _userDbOperations = userDbOperations;
    }

    public async Task Execute(CallbackQuery callbackQuery)
    {
        string? taskId = callbackQuery.Data.Substring("/taskComplete".Length).Trim();
        
        TaskNotification? task = await _taskDbOperations.RetrieveAssignedTask(taskId);
        if (task != null)
        {
            RegisteredUser? user = await _userDbOperations.RetrieveTrelloUser(task.User);

            TrelloOperations trelloOperations = new();
            bool success = await trelloOperations.MarkTaskAsComplete(taskId, user);
            if (success)
            {
                await _botClient.EditMessageTextAsync(text: $"Task: {task.Name} marked as complete",
                    messageId: callbackQuery.Message.MessageId, chatId: callbackQuery.Message.Chat.Id);
                await _taskDbOperations.RemoveAssignedTask(task);
                return;
            }
            
            await _botClient.EditMessageTextAsync(text: $"Task: {task.Name} failed to mark as complete",
                messageId: callbackQuery.Message.MessageId, chatId: callbackQuery.Message.Chat.Id);
        }
    }
}