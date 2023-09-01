using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;
using File = System.IO.File;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

public class AddAttachmentToTask : TaskCreationBaseHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private readonly DisplayCurrentTaskInfo _displayCurrentTaskInfo;

    public AddAttachmentToTask(ITelegramBotClient botClient, 
        UserDbOperations userDbOperations, 
        CreatingTaskDbOperations creatingTaskDbOperations,
        DisplayCurrentTaskInfo displayCurrentTaskInfo, 
        Verifier verifier, 
        BotMessenger botMessenger, TaskDbOperations taskDbOperations) : 
        base(botClient, userDbOperations, verifier, botMessenger, taskDbOperations)
    {
        _botClient = botClient;
        _creatingTaskDbOperations = creatingTaskDbOperations;
        _displayCurrentTaskInfo = displayCurrentTaskInfo;
    }

    protected override async Task HandleTask(User user, TTTTask task)
    {
        if (CallbackQuery is not null)
        {
            if (CallbackQuery.Data is "press_this_when_done")
            {
                await FinishAddingAttachments(user, task);
                return;
            }
        }
        
        (var telegramFilePath, var savedFilePath) = await GetPaths(Message);

        using (Stream fileStream = File.Create(savedFilePath))
        {
            await _botClient.DownloadFileAsync(telegramFilePath, fileStream);
        }

        await _creatingTaskDbOperations.AddFilePath(task,savedFilePath);

        await UpdateBotMessage(user, task);
    }

    private async Task UpdateBotMessage(User user,TTTTask task)
    {
        await BotMessenger.RemoveMessage(chatId: user.TelegramId, messageId: task.LastBotMessage);

        var newMessage = await BotMessenger.SendMessage(text: "Attachment added. You can add more if you want too.",
            chatId: user.TelegramId,
            replyKeyboardMarkup: CreateKeyboard());
        await _creatingTaskDbOperations.MarkMessage(task, newMessage.MessageId);
    }

    private async Task FinishAddingAttachments(User user,TTTTask task)
    {
        if (task.WaitingForAttachment)
        {
            await _creatingTaskDbOperations.WaitingForAttachmentToggle(task);
            NextTask = _displayCurrentTaskInfo;
        }

        await BotMessenger.RemoveMessage(user.TelegramId, CallbackQuery.Message.MessageId);
    }
    
    private async Task<(string, string)> GetPaths(Message message)
    {
        string fileId;
        Telegram.Bot.Types.File fileInfo;
        string? telegramFilePath;
        string savedFilePath = string.Empty;

        if (!Directory.Exists($"./{message.From.Id}"))
        {
            Directory.CreateDirectory($"./{message.From.Id}");
        }
        
        if (message.Photo is not null)
        {
            fileId = message.Photo.Last().FileId;
            fileInfo = await _botClient.GetFileAsync(fileId);
            telegramFilePath = fileInfo.FilePath;
        }
        else if (message.Video is not null)
        {
            fileId = message.Video.FileId;
            fileInfo = await _botClient.GetFileAsync(fileId);
            telegramFilePath = fileInfo.FilePath;
        }
        else
        {
            fileId = message.Document.FileId;
            fileInfo = await _botClient.GetFileAsync(fileId);
            telegramFilePath = fileInfo.FilePath;
        }
        
        int lastDot = telegramFilePath.LastIndexOf(".");
        if (lastDot != -1)
        {
            savedFilePath = $"./{message.From.Id}/" + telegramFilePath.Substring(lastDot - 1);
        }
        
        return (telegramFilePath, savedFilePath);
    }
    
    private InlineKeyboardMarkup CreateKeyboard()
    {
        return new(new[]
        {
            InlineKeyboardButton.WithCallbackData($"Press this when done",
                $"press_this_when_done")
        });
    }
}