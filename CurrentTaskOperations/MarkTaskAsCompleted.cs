using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CurrentTaskOperations;

public class MarkTaskAsCompleted
{
    private CallbackQuery? CallbackQuery { get; set; }
    private ITelegramBotClient BotClient { get; set; }
    private readonly TaskDbOperations _taskDbOperations = new();
    private readonly UserDbOperations _userDbOperations = new();
    
    public MarkTaskAsCompleted(CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        CallbackQuery = callbackQuery;
        BotClient = botClient;
    }

    public async Task Execute()
    {
        string? taskId = CallbackQuery.Data.Substring("/taskComplete".Length).Trim();
        
        TaskNotification? task = await _taskDbOperations.RetrieveAssignedTask(taskId);
        if (task != null)
        {
            RegisteredUser? user = await _userDbOperations.RetrieveTrelloUser(task.User);

            TrelloOperations trelloOperations = new();
            bool success = await trelloOperations.MarkTaskAsComplete(taskId, user);
            if (success)
            {
                await BotClient.EditMessageTextAsync(text: $"Task: {task.Name} marked as complete",
                    messageId: CallbackQuery.Message.MessageId, chatId: CallbackQuery.Message.Chat.Id);
                await _taskDbOperations.RemoveAssignedTask(task);
                return;
            }
            
            await BotClient.EditMessageTextAsync(text: $"Task: {task.Name} failed to mark as complete",
                messageId: CallbackQuery.Message.MessageId, chatId: CallbackQuery.Message.Chat.Id);
        }
    }
}