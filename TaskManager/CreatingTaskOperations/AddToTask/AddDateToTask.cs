using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

public class AddDateToTask : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private readonly DisplayCurrentTaskInfo _displayCurrentTaskInfo;

    public AddDateToTask(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations, DisplayCurrentTaskInfo displayCurrentTaskInfo,
        CreatingTaskDbOperations creatingTaskDbOperations) : base(botClient, userDbOperations, taskDbOperations)
    {
        _displayCurrentTaskInfo = displayCurrentTaskInfo;
        _creatingTaskDbOperations = creatingTaskDbOperations;
        NextTask = displayCurrentTaskInfo;
    }
    
    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        var possibleDate = GetPossibleDate();

        if (possibleDate == null)
        {
            NextTask = null;
            await BotClient.SendTextMessageAsync(text: "Please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                                                       "Due date must be in the future.",
                chatId: Message.Chat.Id);
            return;
        }
        
        await BotClient.DeleteMessageAsync(chatId: Message.Chat.Id, task.LastBotMessage);
        await _creatingTaskDbOperations.AddDate(task, possibleDate);
        
        if (task.InEditMode) await SetNextTaskIfEditMode(task);
    }

    private async Task SetNextTaskIfEditMode(TTTTask task)
    {
        await TaskDbOperations.ToggleEditModeForTask(task);
        
        if (CallbackQuery != null)
        {
            NextTask = _displayCurrentTaskInfo;
            return;
        }

        NextTask = _displayCurrentTaskInfo;
    }

    private string? GetPossibleDate()
    {
        string? possibleDate;
        
        if (CallbackQuery != null)
        {
            string callbackDate = CallbackQuery.Data.Substring("/autodate".Length).Trim();
            possibleDate = DateConverter(callbackDate);
        }
        else
        {
            possibleDate = DateConverter(Message.Text);
        }

        return possibleDate;
    }

    private string? DateConverter(string date)
    {
        DateTime.TryParseExact(date, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None,
            out var properDate);
        if (properDate < DateTime.Now) return null;
        
        return properDate.ToString("o");
    }
}