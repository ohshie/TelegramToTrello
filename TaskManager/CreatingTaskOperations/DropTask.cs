using Telegram.Bot;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class DropTask : TaskCreationBaseHandler
{
    public DropTask(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations) : base(botClient, userDbOperations, taskDbOperations) {}

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        await TaskDbOperations.RemoveEntry(task);
        
        await BotClient.DeleteMessageAsync(chatId: Message.Chat.Id, Message.MessageId);
        await BotClient.SendTextMessageAsync(chatId: Message.Chat.Id,
            text: "Task removed. You can now create a new one");
    }
}