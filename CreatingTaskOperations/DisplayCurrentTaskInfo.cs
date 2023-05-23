using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.CreatingTaskOperations;

public class DisplayCurrentTaskInfo : TaskCreationBaseHandler
{
    private bool IsEdit { get; set; }
    public DisplayCurrentTaskInfo(Message message, ITelegramBotClient botClient) : base(message, botClient) {}

    public DisplayCurrentTaskInfo(CallbackQuery callbackQuery, ITelegramBotClient botClient) : base(
        callbackQuery, botClient) {}

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        var replyMarkup = ReplyKeyboard();
        //if (CallbackQuery != null) await BotClient.DeleteMessageAsync(chatId: CallbackQuery.Message.Chat.Id, CallbackQuery.Message.MessageId);
        
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