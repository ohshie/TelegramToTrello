using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public class TaskNameRequest : TaskCreationBaseHandler
{
    private bool IsEdit { get; set; }
    
    public TaskNameRequest(CallbackQuery callback, ITelegramBotClient botClient, bool isEdit = false) : base(callback, botClient)
    {
        IsEdit = isEdit;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        CreatingTaskDbOperations dbOperations = new(user, task); 
        await dbOperations.AddPlaceholderName();

        if (IsEdit)
        {
            await SendRequestToUser(dbOperations, task);
            return;
        }
        
        await SendRequestToUser(dbOperations, task);
    }

    private async Task SendRequestToUser(DbOperations dbOperations, TTTTask task)
    {
        if (IsEdit) await dbOperations.ToggleEditModeForTask(task);
        await BotClient.DeleteMessageAsync(chatId: CallbackQuery.Message.Chat.Id,
            messageId: CallbackQuery.Message.MessageId);
        await BotClient.SendTextMessageAsync(text: "Now please type name of your task in the next message.",
            chatId: Message.Chat.Id);
    }
}