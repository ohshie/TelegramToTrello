using System.Globalization;
using Elsa.Activities.Temporal;
using Elsa.Builders;
using NodaTime;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.ToFromTrello;

namespace TelegramToTrello.Notifications;

public class BotNotificationCentre : IWorkflow
{
    private readonly NotificationsDbOperations _notificationsDbOperations;
    private readonly TrelloOperations _trelloOperations;
    private IClock _notifyClock;
    private readonly BotMessenger _botMessenger;
    private readonly IConfiguration _configuration;

    public BotNotificationCentre(TrelloOperations trelloOperations, 
        NotificationsDbOperations notificationsDbOperations, IClock clock, BotMessenger botMessenger, IConfiguration configuration)
    {
        _trelloOperations = trelloOperations;
        _notificationsDbOperations = notificationsDbOperations;
        _notifyClock = clock;
        _botMessenger = botMessenger;
        _configuration = configuration;
    }

    public async Task ToggleNotificationsForUser(Message message)
    {
        User? user = await _notificationsDbOperations.ToggleNotifications((int)message.From.Id);
        if (user == null) return;
     
        if (user.NotificationsEnabled)
        {
            await GetCardsForNotifications(user);
            
            await _botMessenger.SendMessage(text: $"Notifications turned on.\n" +
                                                       $"You will now receive messages with new tasks set on your name and when task due is less than 3 hours.\n" +
                                                       $"if you want to disable notifications press /notifications",
            chatId: user.TelegramId);
            return;
        }
        
        await _botMessenger.SendMessage(text: $"Notifications turned off.\n" +
                                                   $"To enable notifications press /notifications",
            chatId: user.TelegramId);
    }

    private async Task<string> GetCardsForNotifications(User trelloUser)
    {
        var cards = await _trelloOperations.GetSubscribedTasks(trelloUser);

        Console.WriteLine($"{trelloUser.TelegramName} updated notifications");
        
        List<TaskNotification> newTasks = await _notificationsDbOperations.UpdateAndAddCards(trelloUser, cards);

        string newTaskMessage = "";
        foreach (var task in newTasks)
        {
            newTaskMessage = $"{newTaskMessage}\n" +
                             $"Name: {task.Name}\n" +
                             $"Due: {task.Due}\n" +
                             $"Link: {task.Url}\n";
        }
        return newTaskMessage;
    }

    public async Task NotificationManager()
    {
        List<User> usersWithNotifications = await _notificationsDbOperations.GetUsersWithNotificationsEnabled();
            
            foreach (var trelloUser in usersWithNotifications) 
            {
                string newTaskMessage = await GetCardsForNotifications(trelloUser);
                if (!string.IsNullOrEmpty(newTaskMessage))
                {
                    await _botMessenger.SendMessage(text: $"Looks like you have some new tasks:\n" +
                                                                $"{newTaskMessage}",
                        chatId: trelloUser.TelegramId);
                }
                
                List<TaskNotification> currentTasks = await _notificationsDbOperations.GetUserCardsFromDb(trelloUser);
           
                foreach (var task in currentTasks)
                {
                    DateTime now = DateTime.Now;
                    DateTime.TryParse(task.Due, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dueDate);
                    TimeSpan timeDelta = dueDate - now;

                    if (timeDelta.TotalHours < 3 && !task.NotificationSent)
                    {
                        await _botMessenger.SendMessage(text: $"You have some tasks that are due soon:\n" +
                                                                    $"{task.Name}\n" +
                                                                    $"{task.Due}\n" +
                                                                    $"{task.Url}",
                            chatId: trelloUser.TelegramId);

                        await _notificationsDbOperations.ToggleSentStatus(task);
                    }
                }
        }
    }

    public void Build(IWorkflowBuilder builder) =>
        builder
            .AsSingleton()
            .Timer(Duration.FromMinutes(_configuration.GetSection("Timers").GetValue<int>("NotificationTimer")))
            .Then(NotificationManager);
}