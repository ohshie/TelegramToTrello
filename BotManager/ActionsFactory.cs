using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.CreatingTaskOperations;
using TelegramToTrello.Notifications;
using TelegramToTrello.TaskManager.CreatingTaskOperations;
using TelegramToTrello.TaskManager.CurrentTaskOperations;
using TelegramToTrello.TemplateManager;
using TelegramToTrello.TemplateManager.CreatingTemplatesOperations;

namespace TelegramToTrello;

public class ActionsFactory
{
    public ActionsFactory(StartTaskCreation startTaskCreation, 
        DropTask dropTask,
        UserRegistrationHandler userRegistrationHandler,
        CurrentTasksDisplay currentTasksDisplay,
        BotSettingsMenu botSettingsMenu,
        TemplateHandler templateHandler,
        BotNotificationCentre botNotificationCentre,
        StartTemplateCreation startTemplateCreation)
    {
        _botTaskFactory = new Dictionary<string, Func<Message, Task>>
        {
            { ActionsList.Register, userRegistrationHandler.Authenticate },
            { ActionsList.Start, userRegistrationHandler.Authenticate },
            
            { ActionsList.NewTask, startTaskCreation.CreateTask },
            { ActionsList.CancelAction, (message) => dropTask.Execute(message)},
            { ActionsList.ShowTasks, currentTasksDisplay.Execute},
            { ActionsList.Settings, botSettingsMenu.Display},
            
            { ActionsList.ManageTemplates, templateHandler.Display },
            { ActionsList.SyncChanges, userRegistrationHandler.SyncBoards },
            { ActionsList.ToggleNotifications, botNotificationCentre.ToggleNotificationsForUser},
            
            { ActionsList.NewTemplate, startTemplateCreation.CreateTemplate },
            { ActionsList.RemoveTemplates, userRegistrationHandler.SyncBoards },
            
            { ActionsList.GoBack, botSettingsMenu.CloseMenu}
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