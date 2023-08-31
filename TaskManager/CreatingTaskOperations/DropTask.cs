using Telegram.Bot;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class DropTask : TaskCreationBaseHandler
{
    private readonly MessageRemover _remover;

    public DropTask(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations, Verifier verifier, MessageRemover remover) : base(botClient, userDbOperations, taskDbOperations, verifier)
    {
        _remover = remover;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        await TaskDbOperations.RemoveEntry(task);

        await _remover.Remove(Message.Chat.Id, Message.MessageId);
        
        await BotClient.SendTextMessageAsync(chatId: Message.Chat.Id,
            text: "Task removed. You can now create a new one");
    }
}