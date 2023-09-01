using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.BotManager;

namespace TelegramToTrello.CreatingTaskOperations;

public class TaskDateRequest : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private readonly DateKeyboard _dateKeyboard;

    public TaskDateRequest(ITelegramBotClient botClient, 
        UserDbOperations userDbOperations,
        CreatingTaskDbOperations creatingTaskDbOperations, Verifier verifier, 
        BotMessenger botMessenger, DateKeyboard dateKeyboard, TaskDbOperations taskDbOperations) : 
        base(botClient, userDbOperations, verifier, botMessenger, taskDbOperations)
    {
        _creatingTaskDbOperations = creatingTaskDbOperations;
        _dateKeyboard = dateKeyboard;
    }

    protected override async Task HandleTask(User user, TTTTask task)
    {
        await _creatingTaskDbOperations.AddPlaceholderDate(task);
        
        if (IsEdit)
        {
            await ToggleEditModeRequestDate(user, task);
            return;
        }
        
        Message newMessage = await BotMessenger.SendMessage(chatId: user.TelegramId,
            text: "All participants added\n\n" +
                  "Now please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                  "Due date must be in the future.",
            replyKeyboardMarkup: _dateKeyboard.CreateKeyboard());
        
        await _creatingTaskDbOperations.MarkMessage(task, newMessage.MessageId);
    }

    private async Task ToggleEditModeRequestDate(User user,TTTTask task)
    {
        await TaskDbOperations.ToggleEditModeForTask(task);

        await BotMessenger.RemoveMessage(user.TelegramId, Message.MessageId);
        
        var newMessage = await BotMessenger.SendMessage(
            text: "Please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                  "Due date must be in the future.",
            chatId: user.TelegramId,
            replyKeyboardMarkup: _dateKeyboard.CreateKeyboard());
        
        await _creatingTaskDbOperations.MarkMessage(task,newMessage.MessageId);
    }
}