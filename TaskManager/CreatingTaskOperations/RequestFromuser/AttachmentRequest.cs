using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.RequestFromuser;

public class AttachmentRequest : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;

    public AttachmentRequest(ITelegramBotClient botClient, 
        UserDbOperations userDbOperations, 
        CreatingTaskDbOperations creatingTaskDbOperations, Verifier verifier, BotMessenger botMessenger,TaskDbOperations taskDbOperations) : 
        base(botClient, userDbOperations, verifier, botMessenger, taskDbOperations)
    {
        _creatingTaskDbOperations = creatingTaskDbOperations;
    }

    protected override async Task HandleTask(TTTTask task)
    {
        await _creatingTaskDbOperations.WaitingForAttachmentToggle(task);

        var botRequest = await BotMessenger.SendMessage(text: "Drag and drop attachment onto bot dialog now",
            chatId: task.Id,
            replyKeyboardMarkup: CreateKeyboard());
        
        await _creatingTaskDbOperations.MarkMessage(task, botRequest.MessageId);
    }

    private InlineKeyboardMarkup CreateKeyboard()
    {
        return new(new[]
        {
            InlineKeyboardButton.WithCallbackData($"Cancel",
                $"Cancel")
        });
    }
}