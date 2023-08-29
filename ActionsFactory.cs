using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;
using TelegramToTrello.Notifications;
using TelegramToTrello.TaskManager.CreatingTaskOperations;
using TelegramToTrello.TaskManager.CurrentTaskOperations;
using TelegramToTrello.TemplateManager;
using TelegramToTrello.UserRegistration;

namespace TelegramToTrello;

public class ActionsFactory
{
    public ActionsFactory(StartTaskCreation startTaskCreation, 
        DropTask dropTask,
        UserRegistrationHandler userRegistrationHandler,
        CurrentTasksDisplay currentTasksDisplay,
        BotSettingsMenu botSettingsMenu,
        TemplateHandler templateHandler,
        BotNotificationCentre botNotificationCentre)
    {
        _botTaskFactory = new Dictionary<string, Func<Message, Task>>
        {
            { "/start", userRegistrationHandler.Authenticate },
            { "/register", userRegistrationHandler.Authenticate },
            
            { "✚ New Task", startTaskCreation.CreateTask },
            { "✁ Cancel action", (message) => dropTask.Execute(message)},
            { "⚅️ Show my tasks", currentTasksDisplay.Execute},
            { "⚙︎ Settings", botSettingsMenu.Display},
            
            {"⚁ Manage templates", templateHandler.Execute },
            {"⚭ Sync changes", userRegistrationHandler.SyncBoards },
            {"⚑ Toggle Notifications", botNotificationCentre.ToggleNotificationsForUser},
            {"✦ Close settings", botSettingsMenu.CloseMenu}
        };
    }

    private readonly Dictionary<string, Func<Message, Task>> _botTaskFactory;
    
    public async Task BotActionFactory(Message message)
    {
        if (message.Text is null) return;

        if (_botTaskFactory.ContainsKey(message.Text))
        {
            await _botTaskFactory[message.Text](message);
        }
    }
}