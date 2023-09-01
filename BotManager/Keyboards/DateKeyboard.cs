using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.BotManager;

public class DateKeyboard
{
    public InlineKeyboardMarkup CreateKeyboard()
    {
        var today = GetTodayDate();
        var endOfTheWeek = GetFriday();

        InlineKeyboardMarkup keyboardMarkup = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"Today",
                    $"/autodate {today.ToString("dd.MM.yyyy HH:mm")}"),
                InlineKeyboardButton.WithCallbackData($"Tomorrow",
                    $"/autodate {today.AddDays(1).ToString("dd.MM.yyyy HH:mm")}"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"Friday",
                    $"/autodate {endOfTheWeek.ToString("dd.MM.yyyy HH:mm")}"),
                InlineKeyboardButton.WithCallbackData($"Next Monday",
                    $"/autodate {endOfTheWeek.AddDays(3).ToString("dd.MM.yyyy HH:mm")}"),
                InlineKeyboardButton.WithCallbackData($"Next Wednesday",
                    $"/autodate {endOfTheWeek.AddDays(5).ToString("dd.MM.yyyy HH:mm")}"),
                InlineKeyboardButton.WithCallbackData($"Next Friday",
                    $"/autodate {endOfTheWeek.AddDays(7).ToString("dd.MM.yyyy HH:mm")}"),
            }
        });

        return keyboardMarkup;
    }

    private static DateTime GetFriday()
    {
        var endOfTheWeek = GetTodayDate();
        switch (endOfTheWeek.DayOfWeek)
        {
            case DayOfWeek.Monday:
            {
                endOfTheWeek = endOfTheWeek.AddDays(4);
                break;
            }
            case DayOfWeek.Tuesday:
            {
                endOfTheWeek =  endOfTheWeek.AddDays(3);
                break;
            }
            case DayOfWeek.Wednesday:
            {
                endOfTheWeek = endOfTheWeek.AddDays(2);
                break;
            }
            case DayOfWeek.Thursday:
            {
                endOfTheWeek = endOfTheWeek.AddDays(1);
                break;
            }
            case DayOfWeek.Saturday:
            {
                endOfTheWeek = endOfTheWeek.AddDays(7);
                break;
            }
            case DayOfWeek.Sunday:
            {
                endOfTheWeek = endOfTheWeek.AddDays(6);
                break;
            }
        }
        
        return endOfTheWeek;
    }

    private static DateTime GetTodayDate()
    {
        DateTime date = DateTime.Today;
        TimeSpan eithteenOClock = new TimeSpan(18, 0, 0);
        date += eithteenOClock;
        return date;
    }
}