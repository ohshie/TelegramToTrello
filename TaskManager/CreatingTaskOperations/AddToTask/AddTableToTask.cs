using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello.TaskManager.CreatingTaskOperations.AddToTask;

public class AddTableToTask : TaskCreationBaseHandler
{
    private readonly CreateKeyboardWithTags _createKeyboardWithTags;
    private readonly CreatingTaskDbOperations _creatingTaskDbOperations;
    private readonly CreateKeyboardWithUsers _createKeyboardWithUsers;

    public AddTableToTask(ITelegramBotClient botClient, UserDbOperations userDbOperations, 
        CreateKeyboardWithTags createKeyboardWithTags,
        CreatingTaskDbOperations creatingTaskDbOperations, 
        CreateKeyboardWithUsers createKeyboardWithUsers,
        Verifier verifier, BotMessenger botMessenger, TaskDbOperations taskDbOperations) : 
        base(botClient, userDbOperations, verifier, botMessenger, taskDbOperations)
    {
        _createKeyboardWithTags = createKeyboardWithTags;
        _creatingTaskDbOperations = creatingTaskDbOperations;
        _createKeyboardWithUsers = createKeyboardWithUsers;
    }

    protected override async Task HandleTask(User user, TTTTask task)
    {
        string listName = IsEdit
            ? CallbackQuery.Data.Substring("/editlist".Length).Trim()
            : CallbackQuery.Data.Substring("/list".Length).Trim();
        
        await _creatingTaskDbOperations.AddTable(task, listName);

        if (IsEdit)
        {
            await TaskDbOperations.ToggleEditModeForTask(task);
            NextTask = _createKeyboardWithUsers;
            NextTask.IsEdit = true;
            return;
        }
       
        NextTask = _createKeyboardWithTags; 
    }
}