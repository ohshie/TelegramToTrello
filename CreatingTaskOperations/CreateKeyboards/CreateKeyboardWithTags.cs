using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.CreatingTaskOperations;

public class CreateKeyboardWithTags : TaskCreationOperator
{
    public CreateKeyboardWithTags(CallbackQuery callback, ITelegramBotClient botClient) : base(callback, botClient) {}

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        InlineKeyboardMarkup replyKeyboardMarkup = KeyboardTagChoice();
        await Task.WhenAll(
            BotClient.EditMessageTextAsync(chatId: CallbackQuery.Message.Chat.Id,
                messageId: CallbackQuery.Message.MessageId, 
                text: $"Choose channel tag according to your task channel"),
            
            BotClient.EditMessageReplyMarkupAsync(chatId: Message.Chat.Id, 
                messageId:CallbackQuery.Message.MessageId,
                replyMarkup: replyKeyboardMarkup));
    }
    
    private InlineKeyboardMarkup KeyboardTagChoice()
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();

        foreach (var tag in Enum.GetValues(typeof(ChanelTags)))
        {
            keyboardButtonsList.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData($"{tag}",$"/tag {tag}") });
        }

        InlineKeyboardMarkup replyKeyboardMarkup = new(keyboardButtonsList);

        return replyKeyboardMarkup;
    }
}