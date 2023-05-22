using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public class TaskDateRequest : TaskCreationBaseHandler
{
    public bool IsEdit { get; set; }

    public TaskDateRequest(CallbackQuery callback, ITelegramBotClient botClient, bool isEdit = false) : base(callback,
        botClient)
    {
        IsEdit = isEdit;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        //if (!task.DescSet) return;

        CreatingTaskDbOperations dbOperations = new(user, task);
        await dbOperations.AddPlaceholderDate();
        if (IsEdit)
        {
            await ToggleEditModeRequestDate(task, dbOperations);
            return;
        }
        
        await BotClient.SendTextMessageAsync(text: "All participants added\n\n" +
                                                   "Now please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                                                   "Due date must be in the future.", 
            chatId: Message.Chat.Id);
    }

    private async Task ToggleEditModeRequestDate(TTTTask task, CreatingTaskDbOperations dbOperations)
    {
        await dbOperations.ToggleEditModeForTask(task);
        await BotClient.DeleteMessageAsync(chatId: CallbackQuery.Message.Chat.Id,
            messageId: CallbackQuery.Message.MessageId);
        await BotClient.SendTextMessageAsync(
            text: "Please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                  "Due date must be in the future.",
            chatId: Message.Chat.Id);
    }
}