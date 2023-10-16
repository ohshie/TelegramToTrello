using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.ToFromTrello;
using File = System.IO.File;

namespace TelegramToTrello.CreatingTaskOperations;

public class PushTask : TaskCreationBaseHandler
{
    private readonly UserDbOperations _userDbOperations;
    private readonly TrelloOperations _trelloOperations;

    public PushTask(ITelegramBotClient botClient,
        UserDbOperations userDbOperations, TrelloOperations trelloOperations,
        Verifier verifier,
        BotMessenger botMessenger, TaskDbOperations taskDbOperations) : 
        base(botClient, userDbOperations, verifier, botMessenger, taskDbOperations)
    {
        _userDbOperations = userDbOperations;
        _trelloOperations = trelloOperations;
    }

    protected override async Task HandleTask(TTTTask task)
    {
        var user = await _userDbOperations.RetrieveTrelloUser(task.Id);
        
        (bool creatingTaskSuccess, string taskId) = await _trelloOperations.PushTaskToTrello(task, user);
        if (!string.IsNullOrEmpty(taskId) && !string.IsNullOrEmpty(task.Attachments))
        {
            await _trelloOperations.AddAttachmentsToTask(task, user, taskId);
        }
        
        if (creatingTaskSuccess)
        {
            await BotMessenger.RemoveMessage(chatId: task.Id, CallbackQuery.Message.MessageId);
            
            await BotMessenger.SendMessage(chatId: task.Id,
                text: "Task successfully created");
            
            await RemoveTaskFromDb(task);
            
            RemoveFiles(task);
        }
    }

    private void RemoveFiles(TTTTask task)
    {
        if (!Directory.Exists($"./{task.Id}")) return;
        var allAttachments = Directory.EnumerateFiles($"./{task.Id}/");
        foreach (var attachment in allAttachments) File.Delete(attachment);
    }

    private async Task RemoveTaskFromDb(TTTTask task)
    {
        await TaskDbOperations.RemoveEntry(task);
    }
}