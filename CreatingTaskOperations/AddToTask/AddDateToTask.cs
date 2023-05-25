using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public class AddDateToTask : TaskCreationBaseHandler
{
    public AddDateToTask(Message message, ITelegramBotClient botClient) : base(message, botClient)
    {
        NextTask = new DisplayCurrentTaskInfo(message, botClient);
    }
    
    public AddDateToTask(CallbackQuery callbackQuery, ITelegramBotClient botClient) : base(callbackQuery, botClient)
    {
        NextTask = new DisplayCurrentTaskInfo(Message, botClient);
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        var possibleDate = GetPossibleDate();

        if (possibleDate == null)
        {
            NextTask = null;
            if (task.InEditMode)
            {
                await BotClient.EditMessageTextAsync(text: "Please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                                                           "Due date must be in the future.",
                    chatId: CallbackQuery.Message.Chat.Id,
                    messageId:task.MessageForDeletionId);
                return;
            }
           
            await BotClient.SendTextMessageAsync(text: "Please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                                                       "Due date must be in the future.",
                chatId: Message.Chat.Id);
            return;
        }

        CreatingTaskDbOperations dbOperations = new(user, task);
        
        await BotClient.DeleteMessageAsync(chatId: Message.Chat.Id, task.MessageForDeletionId);
        await dbOperations.AddDate(possibleDate, Message.MessageId);
        
        if (task.InEditMode) await SetNextTaskIfEditMode(task);
    }

    private async Task SetNextTaskIfEditMode(TTTTask task)
    {
        TaskDbOperations taskDbOperations = new();
        await taskDbOperations.ToggleEditModeForTask(task);
        
        if (CallbackQuery != null)
        {
            NextTask = new DisplayCurrentTaskInfo(CallbackQuery, BotClient);
            return;
        }

        NextTask = new DisplayCurrentTaskInfo(Message, BotClient);
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
        if (properDate < DateTime.Today) return null;
       
        if (DateTime.TryParseExact(date, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out properDate))
        {
            string trelloDate = properDate.ToString("o");
            return trelloDate;
        }
        
        return null;
    }
}