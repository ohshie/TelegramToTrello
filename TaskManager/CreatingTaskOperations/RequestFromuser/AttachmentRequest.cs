using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.RequestFromuser;

public class AttachmentRequest : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;

    public AttachmentRequest(ITelegramBotClient botClient, 
        UserDbOperations dbOperations, 
        TaskDbOperations taskDbOperations,
        CreatingTaskDbOperations creatingTaskDbOperations) : base(botClient, dbOperations, taskDbOperations)
    {
        _creatingTaskDbOperations = creatingTaskDbOperations;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        await _creatingTaskDbOperations.WaitingForAttachmentToggle(task);

        var botRequest = await BotClient.SendTextMessageAsync(text: "Drag and drop attachment onto bot dialog now",
            chatId: CallbackQuery.Message.Chat.Id,
            replyMarkup: CreateKeyboard());
        
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