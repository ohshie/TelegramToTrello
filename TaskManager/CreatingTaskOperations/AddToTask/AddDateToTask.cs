using System.Globalization;
using Telegram.Bot;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

public class AddDateToTask : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private readonly DisplayCurrentTaskInfo _displayCurrentTaskInfo;

    public AddDateToTask(ITelegramBotClient botClient, 
        UserDbOperations userDbOperations, 
        DisplayCurrentTaskInfo displayCurrentTaskInfo,
        CreatingTaskDbOperations creatingTaskDbOperations, Verifier verifier, 
        BotMessenger botMessenger, TaskDbOperations taskDbOperations) : 
        base(botClient, userDbOperations, verifier, botMessenger, taskDbOperations)
    {
        _displayCurrentTaskInfo = displayCurrentTaskInfo;
        _creatingTaskDbOperations = creatingTaskDbOperations;
        NextTask = displayCurrentTaskInfo;
    }
    
    protected override async Task HandleTask(TTTTask task)
    {
        var possibleDate = GetPossibleDate();

        if (possibleDate == null)
        {
            NextTask = null;
            await BotMessenger.SendMessage(text: "Please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                                                       "Due date must be in the future.",
                chatId: task.Id);
            return;
        }
        
        await BotMessenger.RemoveMessage(chatId: task.Id, task.LastBotMessage);
        await _creatingTaskDbOperations.AddDate(task, possibleDate);
        
        if (task.InEditMode)  await TaskDbOperations.ToggleEditModeForTask(task);
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