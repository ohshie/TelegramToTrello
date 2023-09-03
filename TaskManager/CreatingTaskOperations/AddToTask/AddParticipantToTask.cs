using Telegram.Bot;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

public class AddParticipantToTask : TaskCreationBaseHandler
{
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private readonly CreateKeyboardWithUsers _createKeyboardWithUsers;
    private readonly TaskDateRequest _taskDateRequest;
    private readonly DisplayCurrentTaskInfo _displayCurrentTaskInfo;

    public AddParticipantToTask(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        CreatingTaskDbOperations creatingTaskDbOperations, 
        CreateKeyboardWithUsers createKeyboardWithUsers,
        TaskDateRequest taskDateRequest,
        DisplayCurrentTaskInfo displayCurrentTaskInfo, Verifier verifier,
        BotMessenger botMessenger, TaskDbOperations taskDbOperations) : 
        base(botClient, userDbOperations, verifier, botMessenger, taskDbOperations)
    {
        _creatingTaskDbOperations = creatingTaskDbOperations;
        _createKeyboardWithUsers = createKeyboardWithUsers;
        _taskDateRequest = taskDateRequest;
        _displayCurrentTaskInfo = displayCurrentTaskInfo;
    }

    protected override async Task HandleTask(TTTTask task)
    {
        string participantName = CallbackQuery.Data.Substring("/name".Length).Trim();
        if (participantName == "press this when done")
        {
            await FinishAddingParticipants(task);
            return;
        }
        
        await _creatingTaskDbOperations.AddParticipant(task, participantName);
        
        NextTask = _createKeyboardWithUsers;
    }

    private async Task FinishAddingParticipants(TTTTask task)
    {
        await BotMessenger.RemoveMessage(chatId: task.Id, Message.MessageId);
        if (task.InEditMode)
        {
            await TaskDbOperations.ToggleEditModeForTask(task);
            NextTask = _displayCurrentTaskInfo;
            return;
        }
     
        NextTask = _taskDateRequest;
    }
}