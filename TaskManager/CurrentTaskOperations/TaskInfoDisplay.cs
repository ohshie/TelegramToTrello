using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.TaskManager.CurrentTaskOperations;

public class TaskInfoDisplay
{
    private readonly ITelegramBotClient _botClient;
    private readonly TaskDbOperations _taskDbOperations;

    public TaskInfoDisplay(ITelegramBotClient botClient, TaskDbOperations taskDbOperations)
    {
        _botClient = botClient;
        _taskDbOperations = taskDbOperations;
    }

    public async Task Execute(CallbackQuery callbackQuery)
    {
        string? taskId = callbackQuery.Data.Substring("/edittask".Length).Trim();
        
        TaskNotification? task = await _taskDbOperations.RetrieveAssignedTask(taskId);

        var botMessage = BotMessage(task);

        await _botClient.EditMessageTextAsync(text: botMessage,
            messageId: callbackQuery.Message.MessageId, 
            chatId: callbackQuery.Message.Chat.Id,
            replyMarkup: CreateKeyboard(task));
    }

    private static string BotMessage(TaskNotification? task)
    {
        string botMessage = $"Task name: {task.Name}\n" +
                            $"On desc: {task.TaskBoard}, {task.TaskList}\n" +
                            $"Task description: {task.Description} \n" +
                            $"Task due: {task.Due}\n" +
                            $"Url: {task.Url}";
        return botMessage;
    }

    private InlineKeyboardMarkup CreateKeyboard(TaskNotification task)
    {
        InlineKeyboardMarkup keyboard = new(new[]
        {
            InlineKeyboardButton.WithCallbackData($"Mark as complete", $"/taskComplete {task.TaskId}"),
            InlineKeyboardButton.WithCallbackData($"Move to different list", $"/taskMove {task.TaskId}"), 
        });
        return keyboard;
    }
}