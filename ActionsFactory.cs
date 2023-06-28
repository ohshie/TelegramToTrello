using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.CreatingTaskOperations;
using TelegramToTrello.Notifications;
using TelegramToTrello.TaskManager.CreatingTaskOperations;
using TelegramToTrello.TaskManager.CurrentTaskOperations;
using TelegramToTrello.UserRegistration;

namespace TelegramToTrello;

public class ActionsFactory
{
    private readonly StartTaskCreation _startTaskCreation;
    private readonly DropTask _dropTask;
    private readonly UserRegistrationHandler _userRegistrationHandler;
    private readonly BotNotificationCentre _botNotificationCentre;
    private readonly CurrentTasksDisplay _currentTasksDisplay;
    private readonly ITelegramBotClient _botClient;

    public ActionsFactory(StartTaskCreation startTaskCreation, 
        DropTask dropTask,
        UserRegistrationHandler userRegistrationHandler,
        BotNotificationCentre botNotificationCentre,
        CurrentTasksDisplay currentTasksDisplay,
        ITelegramBotClient botClient)
    {
        _startTaskCreation = startTaskCreation;
        _dropTask = dropTask;
        _userRegistrationHandler = userRegistrationHandler;
        _botNotificationCentre = botNotificationCentre;
        _currentTasksDisplay = currentTasksDisplay;
        _botClient = botClient;

        _botTaskFactory = new Dictionary<string, Func<Message, Task>>
        {
            { "/start", (message) => _userRegistrationHandler.Authenticate(message) },
            { "/register", (message) => _userRegistrationHandler.Authenticate(message) },
            { "/SyncBoards", (message) => _userRegistrationHandler.SyncBoards(message) },
            { "🟰Sync changes", (message) => _userRegistrationHandler.SyncBoards(message) },
            { "/newtask", (message) => _startTaskCreation.CreateTask(message) },
            { "➕New Task", (message) => _startTaskCreation.CreateTask(message) },
            { "/notifications", (message) => _botNotificationCentre.ToggleNotificationsForUser(message)},
            { "/drop", (message) =>  _dropTask.Execute(message)},
            { "➖Cancel action", (message) => _dropTask.Execute(message)},
            { "♾️Show my tasks", (message) => _currentTasksDisplay.Execute(message)}
        };
    }

    private readonly Dictionary<string, Func<Message, Task>> _botTaskFactory;
    
    public async Task BotActionFactory(Message message)
    {
        if (_botTaskFactory.ContainsKey(message.Text))
        {
            await _botTaskFactory[message.Text](message);
        }
    }
}