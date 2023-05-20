using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public class AddDateToTask : TaskCreationOperator
{
    public AddDateToTask(Message message, ITelegramBotClient botClient) : base(message, botClient)
    {
        NextTask = new DisplayCurrentTaskInfo(message, botClient);
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        if (Message.Text.StartsWith("/"))
        {
            await BotClient.SendTextMessageAsync(Message.Chat.Id,
                replyToMessageId: Message.MessageId,
                text: $"Task date should not start with \"/\"\n" +
                      $"Please type a new date for a task.");
            return;
        }
        
        string possibleDate = DateConverter(Message.Text);
        if (possibleDate == null)
        {
            await BotClient.SendTextMessageAsync(text: "Please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                                                       "Due date must be in the future.",
                chatId: Message.Chat.Id,
                replyToMessageId: Message.MessageId);
            return;
        }

        CreatingTaskDbOperations dbOperations = new(user, task);
        await dbOperations.AddDateToTask(possibleDate);
    }
    
    private string DateConverter(string date)
    {
        DateTime properDate;
        DateTime.TryParseExact(date, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None,
            out properDate);
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