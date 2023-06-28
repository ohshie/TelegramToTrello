using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public class TaskDescriptionRequest : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;

    public TaskDescriptionRequest(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations, CreatingTaskDbOperations creatingTaskDbOperations) : base(botClient, userDbOperations, taskDbOperations)
    {
        _creatingTaskDbOperations = creatingTaskDbOperations;
    }
    

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        await _creatingTaskDbOperations.AddPlaceholderDescription(task);

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
        await BotClient.DeleteMessageAsync(chatId: CallbackQuery.Message.Chat.Id,
            messageId: CallbackQuery.Message.MessageId);
        await BotClient.SendTextMessageAsync(text: "Now please type name of your task in the next message.",
            chatId: Message.Chat.Id);
    }
}