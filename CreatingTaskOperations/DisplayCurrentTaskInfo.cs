using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.CreatingTaskOperations;

public class DisplayCurrentTaskInfo : TaskCreationOperator
{
    public DisplayCurrentTaskInfo(Message message, ITelegramBotClient botClient) : base(message, botClient) {}

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        var replyMarkup = ReplyKeyboard();
        
        await BotClient.SendTextMessageAsync(text: "Lets review current task:\n\n" +
                                                   $"Task name: [{task.Tag}] {task.TaskName}\n" +
                                                   $"On board: {task.TrelloBoardName}\n"+
                                                   $"Description: {task.TaskDesc}\n"+
                                                   $"Participants: {task.TaskPartName}\n"+
                                                   $"Due date: {DateTime.Parse(task.Date)}\n\n" +
                                                   $"If everything is correct press /push to post this task to trello\n", 
            chatId: Message.Chat.Id, replyMarkup: replyMarkup);
    }

    private InlineKeyboardMarkup ReplyKeyboard()
    {
        InlineKeyboardMarkup replyKeyboardMarkup = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Push task to trello", callbackData:"/push"), 
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Edit name", callbackData:"/push"),
                InlineKeyboardButton.WithCallbackData(text: "Edit description", callbackData:"/push"),
            },
            new[]
            {
            InlineKeyboardButton.WithCallbackData(text: "Edit board/list/part", callbackData:"/push"),
            InlineKeyboardButton.WithCallbackData(text: "Edit task date", callbackData:"/push"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Drop task", callbackData:"/push") 
            }
        });

        return replyKeyboardMarkup;
    }
}