using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.CreatingTaskOperations;

public class CreateKeyboardWithTags : TaskCreationBaseHandler
{
    public CreateKeyboardWithTags(CallbackQuery callback, ITelegramBotClient botClient) : base(callback, botClient) {}

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        InlineKeyboardMarkup replyKeyboardMarkup = KeyboardTagChoice();
        await BotClient.EditMessageTextAsync(chatId: CallbackQuery.Message.Chat.Id,
            messageId: CallbackQuery.Message.MessageId,
            text: $"Choose channel tag according to your task channel", replyMarkup: replyKeyboardMarkup);
    }
    
    private InlineKeyboardMarkup KeyboardTagChoice()
    {
        if (Enum.GetValues(typeof(ChanelTags)).Length > 8)
        {
            InlineKeyboardMarkup replyKeyboardMarkup = new(TwoRowKeyboard());
            return replyKeyboardMarkup;
        }
        else
        {
            
            InlineKeyboardMarkup replyKeyboardMarkup = new(SingleRowKeyboard());
            return replyKeyboardMarkup;
        }
    }
    
    private List<InlineKeyboardButton[]> TwoRowKeyboard()
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();

        var amountOfTags = Enum.GetValues(typeof(ChanelTags)).Length;
        var tags = Enum.GetValues<ChanelTags>();
        for (int i = 0; i < amountOfTags; i +=2)
        {
            if (i < amountOfTags-1)
            {
                keyboardButtonsList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{tags[i]}",
                        $"/tag {tags[i]}"),
                    InlineKeyboardButton.WithCallbackData($"{tags[i+1]}",
                        $"/tag {tags[i+1]}")
                });
            }
            else
            {
                keyboardButtonsList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{tags[i]}",
                        $"/tag {tags[i]}")
                });
            }
        }
        return keyboardButtonsList;
    }

    private List<InlineKeyboardButton[]> SingleRowKeyboard()
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();

        foreach (var tag in Enum.GetValues(typeof(ChanelTags)))
        {
            keyboardButtonsList.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData($"{tag}",$"/tag {tag}") });
        }

        return keyboardButtonsList;
    }
}