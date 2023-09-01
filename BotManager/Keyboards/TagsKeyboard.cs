using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.BotManager;

public class TagsKeyboard
{
    public bool IsTemplate = false;
    private string Callback = CallbackList.Tag;
    
    public InlineKeyboardMarkup KeyboardTagChoice()
    {
        if (IsTemplate)
        {
            Callback = CallbackList.TemplateTag;
        }
        
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
                        $"{Callback} {tags[i]}"),
                    InlineKeyboardButton.WithCallbackData($"{tags[i+1]}",
                        $"{Callback} {tags[i+1]}")
                });
            }
            else
            {
                keyboardButtonsList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{tags[i]}",
                        $"{Callback} {tags[i]}")
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
            keyboardButtonsList.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData($"{tag}",$"{Callback} {tag}") });
        }

        return keyboardButtonsList;
    }
}