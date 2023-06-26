using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.ToFromTrello;

namespace TelegramToTrello.Notifications;

public class BotNotificationCentre
{
    private readonly NotificationsDbOperations _notificationsDbOperations = new();
    private readonly TrelloOperations _trelloOperations = new();
    private readonly ITelegramBotClient _botClient;
    private readonly Message? _message;

    public BotNotificationCentre(Message message, ITelegramBotClient botClient)
    {
        _botClient = botClient;
        _message = message;
    }

    public BotNotificationCentre(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task ToggleNotificationsForUser()
    {
        RegisteredUser? user = await _notificationsDbOperations.ToggleNotifications((int)_message.From.Id);
        if (user == null) return;
     
        if (user.NotificationsEnabled)
        {
            await GetCardsForNotifications(user);
            
            await _botClient.SendTextMessageAsync(text: $"Notifications turned on.\n" +
                                                       $"You will now receive messages with new tasks set on your name and when task due is less than 3 hours.\n" +
                                                       $"if you want to disable notifications press /notifications",
            chatId: user.TelegramId);
            return;
        }
        
        await _botClient.SendTextMessageAsync(text: $"Notifications turned off.\n" +
                                                   $"To enable notifications press /notifications",
            chatId: user.TelegramId);
    }

    private async Task<string> GetCardsForNotifications(RegisteredUser trelloUser)
    {
        var cards = await _trelloOperations.GetSubscribedTasks(trelloUser);

        Console.WriteLine("done");
        
        List<TaskNotification> newTasks = await _notificationsDbOperations.UpdateAndAddCards(trelloUser, cards);

        string newTaskMessage = "";
        foreach (var task in newTasks)
        {
            newTaskMessage = $"{newTaskMessage}\n" +
                             $"Name: {task.Name}\n" +
                             $"Due: {DateTime.Parse(task.Due)}\n" +
                             $"Link: {task.Url}\n";
        }
        return newTaskMessage;
    }

    public async Task NotificationManager()
    {
       List<RegisteredUser> usersWithNotifications = await _notificationsDbOperations.GetUsersWithNotificationsEnabled();

       foreach (var trelloUser in usersWithNotifications)
       {
           string newTaskMessage = await GetCardsForNotifications(trelloUser);
           if (!string.IsNullOrEmpty(newTaskMessage))
           {
               await _botClient.SendTextMessageAsync(text: $"Looks like you have some new tasks:\n" +
                                                    $"{newTaskMessage}",
                   chatId: trelloUser.TelegramId);
           }

           List<TaskNotification> currentTask = await _notificationsDbOperations.GetUserCardsFromDb(trelloUser);

           foreach (var task in currentTask)
           {
               DateTime now = DateTime.Now;
               DateTime dueDate = DateTime.Parse(task.Due);
               TimeSpan timeDelta = dueDate - now;
               
               if (timeDelta.TotalHours < 3 && timeDelta.TotalHours > 1)
               {
                   
                   await _botClient.SendTextMessageAsync(text: $"You have some tasks that are due soon:\n" +
                                                              $"{task.Name}\n" +
                                                              $"{DateTime.Parse(task.Due)}\n" +
                                                              $"{task.Url}",
                       chatId: trelloUser.TelegramId);
               }
           }
       }
    }
}