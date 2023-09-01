using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.ToFromTrello;
using File = System.IO.File;

namespace TelegramToTrello.CreatingTaskOperations;

public class PushTask : TaskCreationBaseHandler
{
    private readonly TrelloOperations _trelloOperations;

    public PushTask(ITelegramBotClient botClient,
        UserDbOperations userDbOperations, TrelloOperations trelloOperations,
        Verifier verifier,
        BotMessenger botMessenger, TaskDbOperations taskDbOperations) : 
        base(botClient, userDbOperations, verifier, botMessenger, taskDbOperations)
    {
        _trelloOperations = trelloOperations;
    }

    protected override async Task HandleTask(User user, TTTTask task)
    {
        (bool creatingTaskSuccess, string taskId) = await _trelloOperations.PushTaskToTrello(task, user);
        if (!string.IsNullOrEmpty(taskId) && !string.IsNullOrEmpty(task.Attachments))
        {
            await _trelloOperations.AddAttachmentsToTask(task, user, taskId);
        }
        
        if (creatingTaskSuccess)
        {
            await BotMessenger.RemoveMessage((int)Message.Chat.Id, CallbackQuery.Message.MessageId);
            
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