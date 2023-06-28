using Telegram.Bot;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

public class AddParticipantToTask : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private readonly CreateKeyboardWithUsers _createKeyboardWithUsers;
    private readonly TaskDateRequest _taskDateRequest;
    private readonly DisplayCurrentTaskInfo _displayCurrentTaskInfo;

    public AddParticipantToTask(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TaskDbOperations taskDbOperations, 
        CreatingTaskDbOperations creatingTaskDbOperations, 
        CreateKeyboardWithUsers createKeyboardWithUsers,
        TaskDateRequest taskDateRequest,
        DisplayCurrentTaskInfo displayCurrentTaskInfo) : base(botClient, userDbOperations, taskDbOperations)
    {
        _creatingTaskDbOperations = creatingTaskDbOperations;
        _createKeyboardWithUsers = createKeyboardWithUsers;
        _taskDateRequest = taskDateRequest;
        _displayCurrentTaskInfo = displayCurrentTaskInfo;
    }

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        string participantName = CallbackQuery.Data.Substring("/name".Length).Trim();
        if (participantName == "press this when done")
        {
            await FinishAddingParticipants(task);
            return;
        }
        
        bool userFoundOnBoard = await _creatingTaskDbOperations.AddParticipant(task, participantName);
        if (!userFoundOnBoard)
        {
            await BotClient.SendTextMessageAsync(text: "Please choose name from keyboard menu.",
                chatId: Message.Chat.Id);
            return;
        }
        
        NextTask = _createKeyboardWithUsers;
    }

    private async Task FinishAddingParticipants(TTTTask task)
    {
        await BotClient.DeleteMessageAsync(chatId: CallbackQuery.Message.Chat.Id, CallbackQuery.Message.MessageId);
        if (task.InEditMode)
        {
            await TaskDbOperations.ToggleEditModeForTask(task);
            
            NextTask = _displayCurrentTaskInfo;
        }
        else
        {
            NextTask = _taskDateRequest;
        }
    }
}