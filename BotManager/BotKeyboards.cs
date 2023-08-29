using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.BotManager;

public class BotKeyboards
{
    public ReplyKeyboardMarkup MainKeyboard()
    {
        ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] {"✚ New Task", "✁ Cancel action"},
            new KeyboardButton[] {"⚅️ Show my tasks", "⚙︎ Settings"}
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = false,
            IsPersistent = true
        };
        return keyboard;
    }

    public ReplyKeyboardMarkup SettingsKeyboard()
    {
        ReplyKeyboardMarkup keyboardMarkup = new ReplyKeyboardMarkup(
            new []
            {
                new KeyboardButton[] {"⚁ Manage templates", "⚭ Sync changes"},
                new KeyboardButton[] { "⚑ Toggle Notifications", "✦ Close settings"}
            })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = false,
            IsPersistent = true
        };;
        return keyboardMarkup;
    }
}