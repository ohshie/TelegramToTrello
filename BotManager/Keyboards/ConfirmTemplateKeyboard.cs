using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.BotManager;

public class ConfirmTemplateKeyboard
{
    public InlineKeyboardMarkup AssembleKeyboard()
    {
        InlineKeyboardMarkup replyKeyboardMarkup = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Save template", callbackData: CallbackList.TemplateSave), 
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Edit name", callbackData: CallbackList.TemplateEditName),
                InlineKeyboardButton.WithCallbackData(text: "Edit description", callbackData: CallbackList.TemplateEditDesc),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Edit Board and list", callbackData: CallbackList.TemplateEditBoard),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Discard template", callbackData: CallbackList.TemplateRemove) 
            }
        });

        return replyKeyboardMarkup;
    }
}