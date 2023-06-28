using Telegram.Bot;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.RequestFromuser;

public class TaskNameRequest : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;

    public TaskNameRequest(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations, CreatingTaskDbOperations creatingTaskDbOperations) : base(botClient, userDbOperations, taskDbOperations)
    {
        _creatingTaskDbOperations = creatingTaskDbOperations;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        await _creatingTaskDbOperations.AddPlaceholderName(task);
        
        await SendRequestToUser(task);
    }

    private async Task SendRequestToUser(TTTTask task)
    {
        if (IsEdit)
        {
            await TaskDbOperations.ToggleEditModeForTask(task);
        }
        
        await BotClient.DeleteMessageAsync(chatId: CallbackQuery.Message.Chat.Id,
            messageId: CallbackQuery.Message.MessageId);
        await BotClient.SendTextMessageAsync(text: "Now please type name of your task in the next message.",
            chatId: Message.Chat.Id);
    }
}