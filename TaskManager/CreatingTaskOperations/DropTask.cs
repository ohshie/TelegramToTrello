using Telegram.Bot;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class DropTask : TaskCreationBaseHandler
{
    public DropTask(ITelegramBotClient botClient, UserDbOperations userDbOperations, 
        Verifier verifier, BotMessenger botMessenger, TaskDbOperations taskDbOperations) : 
        base(botClient, userDbOperations, verifier, botMessenger, taskDbOperations)
    {}

    protected override async Task HandleTask(User user, TTTTask task)
    {
        await TaskDbOperations.RemoveEntry(task);

        await BotMessenger.RemoveLastBotMessage(user.TelegramId);
        await BotMessenger.RemoveMessage(user.TelegramId, Message.MessageId);
        
        await BotMessenger.SendMessage(chatId: (int)Message.Chat.Id,
            text: "Task removed. You can now create a new one");
    }
}