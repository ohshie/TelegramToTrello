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
        Verifier verifier, DisplayTaskKeyboard displayTaskKeyboard, 
        BotMessenger botMessenger, TaskDbOperations taskDbOperations) 
        : base(botClient, userDbOperations, verifier, botMessenger, taskDbOperations)
    {
        _displayTaskKeyboard = displayTaskKeyboard;
    }


    protected override async Task HandleTask(TTTTask task)
    {
        var replyMarkup = _displayTaskKeyboard.ReplyKeyboard();
        
        await BotMessenger.RemoveMessage(chatId: task.Id, Message.MessageId);
        
        await BotMessenger.SendMessage(text: "Lets review current task:\n\n" +
                                                   $"Task name: [{task.Tag}] {task.TaskName}\n" +
                                                   $"On board: {task.TrelloBoardName}\n"+
                                                   $"Description: {task.TaskDesc}\n"+
                                                   $"Participants: {task.TaskPartName}\n"+
                                                   $"Due date: {DateTime.Parse(task.Date, CultureInfo.InvariantCulture)}\n\n" +
                                                   $"If everything is correct press push to post this task to trello\n", 
            chatId: task.Id, 
            replyKeyboardMarkup: replyMarkup);
    }
}