using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public class TaskDateRequest : TaskCreationOperator
{
    public TaskDateRequest(CallbackQuery callback, ITelegramBotClient botClient) : base(callback, botClient)
    {}

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        if (!task.DescSet) return;

        CreatingTaskDbOperations dbOperations = new(user, task);
        await dbOperations.AddPlaceholderDate();
        
        await BotClient.SendTextMessageAsync(text: "All participants added\n\n" +
                                                   "Now please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                                                   "Due date must be in the future.", 
            chatId: Message.Chat.Id);
    }
}