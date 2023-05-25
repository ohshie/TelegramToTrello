using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.CreatingTaskOperations;

public class TaskDateRequest : TaskCreationBaseHandler
{
    private bool IsEdit { get; set; }

    public TaskDateRequest(CallbackQuery callback, ITelegramBotClient botClient, bool isEdit = false) : base(callback,
        botClient)
    {
        IsEdit = isEdit;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        CreateKeyboard();
        CreatingTaskDbOperations dbOperations = new(user, task);
        await dbOperations.AddPlaceholderDate();
        
        if (IsEdit)
        {
            await ToggleEditModeRequestDate(task, dbOperations);
            return;
        }
        
        var newMessage = await BotClient.SendTextMessageAsync(text: "All participants added\n\n" +
                                                   "Now please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                                                   "Due date must be in the future.", 
            chatId: Message.Chat.Id,
            replyMarkup: CreateKeyboard());
        await dbOperations.MarkMessageForDeletion(newMessage.MessageId);
    }

    private async Task ToggleEditModeRequestDate(TTTTask task, CreatingTaskDbOperations creatingTaskDbOperations)
    {
        TaskDbOperations taskDbOperations = new();
        await taskDbOperations.ToggleEditModeForTask(task);
        
        await BotClient.DeleteMessageAsync(chatId: CallbackQuery.Message.Chat.Id,
            messageId: CallbackQuery.Message.MessageId);
        
        var newMessage = await BotClient.SendTextMessageAsync(
            text: "Please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                  "Due date must be in the future.",
            chatId: Message.Chat.Id,
            replyMarkup: CreateKeyboard());
        
        await creatingTaskDbOperations.MarkMessageForDeletion(newMessage.MessageId);
    }

    private InlineKeyboardMarkup CreateKeyboard()
    {
        var today = GetTodayDate();
        var endOfTheWeek = GetFriday();

        InlineKeyboardMarkup keyboardMarkup = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"Today",
                    $"/autodate {today.ToString().Substring(0,today.ToString().Length-3)}"),
                InlineKeyboardButton.WithCallbackData($"Tomorrow",
                    $"/autodate {today.AddDays(1).ToString().Substring(0,today.ToString().Length-3)}"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"Friday",
                    $"/autodate {endOfTheWeek.ToString().Substring(0,today.ToString().Length-3)}"),
                InlineKeyboardButton.WithCallbackData($"Next Monday",
                    $"/autodate {endOfTheWeek.AddDays(3).ToString().Substring(0,today.ToString().Length-3)}"),
                InlineKeyboardButton.WithCallbackData($"Next Wednesday",
                    $"/autodate {endOfTheWeek.AddDays(5).ToString().Substring(0,today.ToString().Length-3)}"),
                InlineKeyboardButton.WithCallbackData($"Next Friday",
                    $"/autodate {endOfTheWeek.AddDays(7).ToString().Substring(0,today.ToString().Length-3)}"),
            }
        });

        return keyboardMarkup;
    }

    private static DateTime GetFriday()
    {
        var endOfTheWeek = DateTime.Today;
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

        TimeSpan eithteenOClock = new TimeSpan(18, 0, 0);
        endOfTheWeek += eithteenOClock;
        return endOfTheWeek;
    }

    private static DateTime GetTodayDate()
    {
        DateTime date = DateTime.Today;
        Console.WriteLine("keyboard date" + date);
        TimeSpan eithteenOClock = new TimeSpan(18, 0, 0);
        date += eithteenOClock;

        var properDate = date.ToString();
        
        DateTime.TryParseExact(properDate.Substring(0,properDate.Length - 3), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out date);
        Console.WriteLine("keyboard new date" + date);
        return date;
    }
}