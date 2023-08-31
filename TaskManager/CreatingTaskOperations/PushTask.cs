using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.ToFromTrello;
using File = System.IO.File;

namespace TelegramToTrello.CreatingTaskOperations;

public class PushTask : TaskCreationBaseHandler
{
    private readonly TrelloOperations _trelloOperations;
    private readonly MessageRemover _messageRemover;

    public PushTask(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations, TrelloOperations trelloOperations,
        Verifier verifier,
        MessageRemover messageRemover) : base(botClient, userDbOperations, taskDbOperations, verifier)
    {
        _trelloOperations = trelloOperations;
        _messageRemover = messageRemover;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        (bool creatingTaskSuccess, string taskId) = await _trelloOperations.PushTaskToTrello(task, user);
        if (!string.IsNullOrEmpty(taskId) && !string.IsNullOrEmpty(task.Attachments))
        {
            await _trelloOperations.AddAttachmentsToTask(task, user, taskId);
        }
        
        if (creatingTaskSuccess)
        {
            await _messageRemover.Remove(CallbackQuery.Message.Chat.Id, CallbackQuery.Message.MessageId);
            
            await BotClient.SendTextMessageAsync(Message.Chat.Id,
                text: "Task successfully created");
            
            await RemoveTaskFromDb(task);
            
            RemoveFiles(task);
        }
    }

    private void RemoveFiles(TTTTask task)
    {
        var allAttachments = Directory.EnumerateFiles($"./{task.Id}/");
        foreach (var attachment in allAttachments) File.Delete(attachment);
    }

    private async Task RemoveTaskFromDb(TTTTask task)
    {
        await TaskDbOperations.RemoveEntry(task);
    }
}