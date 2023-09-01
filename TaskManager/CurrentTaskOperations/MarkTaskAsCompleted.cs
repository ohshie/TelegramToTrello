using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.ToFromTrello;

namespace TelegramToTrello.TaskManager.CurrentTaskOperations;

public class MarkTaskAsCompleted
{
    private readonly ITelegramBotClient _botClient;
    private readonly UserDbOperations _userDbOperations;
    private readonly TrelloOperations _trelloOperations;
    private readonly NotificationsDbOperations _notificationsDbOperations;

    public MarkTaskAsCompleted(ITelegramBotClient botClient,
        UserDbOperations userDbOperations,
        TrelloOperations trelloOperations,
        NotificationsDbOperations notificationsDbOperations)
    {
        _botClient = botClient;
        _userDbOperations = userDbOperations;
        _trelloOperations = trelloOperations;
        _notificationsDbOperations = notificationsDbOperations;
    }

    public async Task Execute(CallbackQuery callbackQuery)
    {
        string? taskId = callbackQuery.Data.Substring("/taskComplete".Length).Trim();
        
        TaskNotification? task = await _notificationsDbOperations.RetrieveAssignedTask(taskId);
        if (task != null)
        {
            User? user = await _userDbOperations.RetrieveTrelloUser(task.User);
            
            bool success = await _trelloOperations.MarkTaskAsComplete(taskId, user);
            if (success)
            {
                await _botClient.EditMessageTextAsync(text: $"Task: {task.Name} marked as complete",
                    messageId: callbackQuery.Message.MessageId, chatId: callbackQuery.Message.Chat.Id);
                await _notificationsDbOperations.RemoveAssignedTask(task);
                return;
            }
            
            await _botClient.EditMessageTextAsync(text: $"Task: {task.Name} failed to mark as complete",
                messageId: callbackQuery.Message.MessageId, chatId: callbackQuery.Message.Chat.Id);
        }
    }
}