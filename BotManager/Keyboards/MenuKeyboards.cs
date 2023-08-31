using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.BotManager;

public class MenuKeyboards
{
    public ReplyKeyboardMarkup MainKeyboard()
    {
        ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] {ActionsList.NewTask, ActionsList.CancelAction},
            new KeyboardButton[] {ActionsList.ShowTasks, ActionsList.Settings}
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
                new KeyboardButton[] {ActionsList.ManageTemplates, ActionsList.SyncChanges},
                new KeyboardButton[] { ActionsList.ToggleNotifications, ActionsList.GoBack}
            })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = false,
            IsPersistent = true
        };;
        return keyboardMarkup;
    }

    public ReplyKeyboardMarkup TemplatesKeyboard()
    {
        ReplyKeyboardMarkup keyboardMarkup = new ReplyKeyboardMarkup(
            new[]
            {
                new KeyboardButton(ActionsList.NewTemplate),
                new KeyboardButton(ActionsList.RemoveTemplates),
                new KeyboardButton(ActionsList.GoBack),
            });

        return keyboardMarkup;
    }
}