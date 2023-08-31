using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations;

public class DisplayCurrentTaskInfo : TaskCreationBaseHandler
{
    private readonly DisplayTaskKeyboard _displayTaskKeyboard;

    public DisplayCurrentTaskInfo(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations, Verifier verifier, DisplayTaskKeyboard displayTaskKeyboard) : base(botClient, userDbOperations, taskDbOperations, verifier)
    {
        _displayTaskKeyboard = displayTaskKeyboard;
    }


    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        var replyMarkup = _displayTaskKeyboard.ReplyKeyboard();
        
        await BotClient.SendTextMessageAsync(text: "Lets review current task:\n\n" +
                                                   $"Task name: [{task.Tag}] {task.TaskName}\n" +
                                                   $"On board: {task.TrelloBoardName}\n"+
                                                   $"Description: {task.TaskDesc}\n"+
                                                   $"Participants: {task.TaskPartName}\n"+
                                                   $"Due date: {DateTime.Parse(task.Date, CultureInfo.InvariantCulture)}\n\n" +
                                                   $"If everything is correct press push to post this task to trello\n", 
            chatId: Message.Chat.Id, replyMarkup: replyMarkup);
    }
}