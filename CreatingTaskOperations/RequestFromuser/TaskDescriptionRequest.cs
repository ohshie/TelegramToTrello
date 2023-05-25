using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public class TaskDescriptionRequest : TaskCreationBaseHandler
{
    private bool IsEdit { get; set; }

    public TaskDescriptionRequest(Message message, ITelegramBotClient botClient) : base(message,
        botClient) {}
    
    public TaskDescriptionRequest(CallbackQuery callbackQuery, ITelegramBotClient botClient, bool isEdit = false) : base(callbackQuery,
        botClient)
    {
        IsEdit = isEdit;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        CreatingTaskDbOperations dbOperations = new(user, task);
        await dbOperations.AddPlaceholderDescription();

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
        TaskDbOperations taskDbOperations = new();
        await taskDbOperations.ToggleEditModeForTask(task);
        await BotClient.DeleteMessageAsync(chatId: CallbackQuery.Message.Chat.Id,
            messageId: CallbackQuery.Message.MessageId);
        await BotClient.SendTextMessageAsync(text: "Now please type name of your task in the next message.",
            chatId: Message.Chat.Id);
    }
}