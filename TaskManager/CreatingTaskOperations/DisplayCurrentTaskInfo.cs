using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class DisplayCurrentTaskInfo : TaskCreationBaseHandler
{
    public DisplayCurrentTaskInfo(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations) : base(botClient, userDbOperations, taskDbOperations) {}


    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        var replyMarkup = ReplyKeyboard();
        
        await BotClient.SendTextMessageAsync(text: "Lets review current task:\n\n" +
                                                   $"Task name: [{task.Tag}] {task.TaskName}\n" +
                                                   $"On board: {task.TrelloBoardName}\n"+
                                                   $"Description: {task.TaskDesc}\n"+
                                                   $"Participants: {task.TaskPartName}\n"+
                                                   $"Due date: {DateTime.Parse(task.Date, CultureInfo.InvariantCulture)}\n\n" +
                                                   $"If everything is correct press push to post this task to trello\n", 
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
                InlineKeyboardButton.WithCallbackData(text: "Edit name", callbackData:"/editname"),
                InlineKeyboardButton.WithCallbackData(text: "Edit description", callbackData:"/editdesc"),
            },
            new[]
            {
            InlineKeyboardButton.WithCallbackData(text: "Edit board/list/part", callbackData:"/edittaskboardandtable"),
            InlineKeyboardButton.WithCallbackData(text: "Edit task date", callbackData:"/editdate"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Drop task", callbackData:"/drop") 
            }
        });

        return replyKeyboardMarkup;
    }
}