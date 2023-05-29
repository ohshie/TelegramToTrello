using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.ToFromTrello;
using TelegramToTrello.UserRegistration;

namespace TelegramToTrello.BotActions;

public class BotNotificationCentre
{
    private readonly NotificationsDbOperations _notificationsDbOperations = new();
    private readonly UserDbOperations _dbOperations = new();
    private readonly TrelloOperations _trelloOperations = new();
    private ITelegramBotClient BotClient { get; }
    private Message? Message { get; }

    public BotNotificationCentre(Message message, ITelegramBotClient botClient)
    {
        BotClient = botClient;
        Message = message;
    }

    public BotNotificationCentre(ITelegramBotClient botClient)
    {
        BotClient = botClient;
    }

    public async Task ToggleNotificationsForUser()
    {
        RegisteredUser? user = await _notificationsDbOperations.ToggleNotifications((int)Message.From.Id);
        if (user == null) return;
     
        if (user.NotificationsEnabled)
        {
            await GetCardsForNotifications(user);
            
            await BotClient.SendTextMessageAsync(text: $"Notifications turned on.\n" +
                                                       $"You will now receive messages with new tasks set on your name and when task due is less than 3 hours.\n" +
                                                       $"if you want to disable notifications press /notifications",
            chatId: user.TelegramId);
            return;
        }
        
        await BotClient.SendTextMessageAsync(text: $"Notifications turned off.\n" +
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
               await BotClient.SendTextMessageAsync(text: $"Looks like you have some new tasks:\n" +
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
                   
                   await BotClient.SendTextMessageAsync(text: $"You have some tasks that are due soon:\n" +
                                                              $"{task.Name}\n" +
                                                              $"{DateTime.Parse(task.Due)}\n" +
                                                              $"{task.Url}",
                       chatId: trelloUser.TelegramId);
               }
           }
       }
    }
}