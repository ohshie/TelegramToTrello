using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.BotManager;

public class DisplayTaskKeyboard
{
    public InlineKeyboardMarkup ReplyKeyboard()
    {
        InlineKeyboardMarkup replyKeyboardMarkup = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Push task to trello", callbackData: CallbackList.Push), 
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Edit name", callbackData: CallbackList.TaskEditname),
                InlineKeyboardButton.WithCallbackData(text: "Edit description", callbackData: CallbackList.TaskEditdesc),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Edit board/list/part", callbackData: CallbackList.EditTaskBoardAndTable),
                InlineKeyboardButton.WithCallbackData(text: "Edit task date", callbackData: CallbackList.TaskEditdate),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Add attachment (image or file)", callbackData: CallbackList.AddAttachment) 
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Drop task", callbackData: CallbackList.Drop) 
            }
        });

        return replyKeyboardMarkup;
    }
}