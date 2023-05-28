namespace TelegramToTrello;

public class NotificationsDbOperations
{
    public async Task<bool> ToggleNotifications(RegisteredUser user)
    {
        using BotDbContext dbContext = new BotDbContext();
        {
            user.NotificationsEnabled = !user.NotificationsEnabled;
            dbContext.Update(user);
            await dbContext.SaveChangesAsync();
            return user.NotificationsEnabled;
        }
    }
    
    public async Task<List<TaskNotification>> UpdateAndAddCards(RegisteredUser user, Dictionary<string, TrelloOperations.TrelloCard> cards)
    {
        using BotDbContext dbContext = new BotDbContext();
        {
            await RemoveTasksThatAreNotInTrello(dbContext, user, cards);
            
            List<TaskNotification> newTasks = new List<TaskNotification>();

            var currentNotifications = dbContext.TaskNotifications.ToDictionary(tn => tn.TaskId);
            var newNotificationsKeys = cards.Keys.Except(currentNotifications.Keys);
            
            if (newNotificationsKeys.Any())
            {
                List<TaskNotification> createNewNotificationsList = new();
                foreach (var key in newNotificationsKeys)
                {
                    string correctDueDate = DateTime.Parse(cards[key].Due).ToUniversalTime().ToString("o");
                    
                    TaskNotification newNotification = new()
                    {
                        TaskId = cards[key].Id,
                        Name = cards[key].Name,
                        Due = correctDueDate,
                        Url = cards[key].Url,
                        User = user.TelegramId,
                        Description = cards[key].Description,
                        Participants = cards[key].Members,
                        BoardId = cards[key].BoardId,
                        ListId = cards[key].ListId
                    };
                    createNewNotificationsList.Add(newNotification);
                }
            }
            
            dbContext.TaskNotifications.AddRange(newTasks);
            await dbContext.SaveChangesAsync();
            
            return newTasks;
        }
    }

    public async Task<List<TaskNotification>> GetUserCardsFromDb(RegisteredUser trelloUser)
    {
        using (BotDbContext dbContext = new BotDbContext())
        {
            List<TaskNotification> allTasks = new List<TaskNotification>();
            allTasks = dbContext.TaskNotifications.Where(tn => tn.User == trelloUser.TelegramId).ToList();

            return allTasks;
        }
    }

    private async Task RemoveTasksThatAreNotInTrello(BotDbContext dbContext, RegisteredUser user, Dictionary<string, TrelloOperations.TrelloCard> cards)
    {
        List<TaskNotification> notificationsList = dbContext.TaskNotifications.Where(tn => tn.User == user.TelegramId).ToList();
        List<string> cardsIds = cards.Values.Select(c => c.Id).ToList();
            
        notificationsList.RemoveAll(item => cardsIds.Contains(item.TaskId));
        dbContext.TaskNotifications.RemoveRange(notificationsList);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<RegisteredUser>> GetUsersWithNotificationsEnabled()
    {
        using BotDbContext dbContext = new BotDbContext();
        {
            List<RegisteredUser> registeredUsers = dbContext.Users.Where(u => u.NotificationsEnabled == true).ToList();
            return registeredUsers;
        }
    }
}