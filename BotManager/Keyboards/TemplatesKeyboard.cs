using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.BotManager;

public class TemplatesKeyboard
{
    //
    
    public InlineKeyboardMarkup KeyboardMarkup(List<Template> templates)
    {
        List<InlineKeyboardButton> keyboardButtons = new(new[]
        {
            InlineKeyboardButton
                .WithCallbackData(text: "Skip template", callbackData: CallbackList.Skip)
        });

        foreach (var template in templates)
        {
            keyboardButtons.Add(InlineKeyboardButton
                .WithCallbackData(text: $"{template.TaskName}",
                    callbackData: CallbackList.Template + $" {template.Id}"));
        };
        
        return new InlineKeyboardMarkup(keyboardButtons);
    }
}