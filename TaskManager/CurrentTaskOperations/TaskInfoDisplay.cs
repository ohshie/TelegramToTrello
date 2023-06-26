using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.CurrentTaskOperations;

public class TaskInfoDisplay
{
    private CallbackQuery? CallbackQuery { get; set; }
    private ITelegramBotClient BotClient { get; set; }
    
    public TaskInfoDisplay(CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        CallbackQuery = callbackQuery;
        BotClient = botClient;
    }

    public async Task Execute()
    {
        string? taskId = CallbackQuery.Data.Substring("/edittask".Length).Trim();

        TaskDbOperations taskDbOperations = new();
        TaskNotification? task = await taskDbOperations.RetrieveAssignedTask(taskId);

        var botMessage = BotMessage(task);

        await BotClient.EditMessageTextAsync(text: botMessage,
            messageId: CallbackQuery.Message.MessageId, 
            chatId: CallbackQuery.Message.Chat.Id,
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