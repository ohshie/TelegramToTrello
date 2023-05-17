using TelegramToTrello;

public class NotificationsDbOperations
{
    public async Task<bool> ToggleNotifications(RegisteredUser user)
    {
        using BotDbContext dbContext = new BotDbContext();
        {
            if (user.NotificationsEnabled == false)
            {
                user.NotificationsEnabled = true;
                dbContext.Update(user);
                await dbContext.SaveChangesAsync();
                return true;
            }
           
            user.NotificationsEnabled = false;
            dbContext.Update(user);
            await dbContext.SaveChangesAsync();
            return false;
        }
    }
    public async Task<List<TaskNotification>> UpdateAndAddCards(RegisteredUser user, List<TrelloOperations.TrelloCard> cards)
    {
        using BotDbContext dbContext = new BotDbContext();
        {
            await RemoveTasksThatAreNotInTrello(dbContext, user, cards);
            
            List<TaskNotification> newTasks = new List<TaskNotification>();
            
            foreach (var card in cards)
            {
                TaskNotification notification = await dbContext.TaskNotifications.FindAsync(card.Id);
                if (notification == null)
                {
                    string correctDueDate = DateTime.Parse(card.Due).ToUniversalTime().ToString("o");
                    
                    notification = new TaskNotification()
                    {
                        Id = card.Id,
                        Name = card.Name,
                        Due = correctDueDate,
                        Url = card.Url,
                        User = user.TelegramId
                    };
                    newTasks.Add(notification);
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

    private async Task RemoveTasksThatAreNotInTrello(BotDbContext dbContext, RegisteredUser user, List<TrelloOperations.TrelloCard> cards)
    {
        List<TaskNotification> notificationsList = dbContext.TaskNotifications.Where(tn => tn.User == user.TelegramId).ToList();
        List<string> cardsIds = cards.Select(c => c.Id).ToList();
            
        notificationsList.RemoveAll(item => cardsIds.Contains(item.Id));
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