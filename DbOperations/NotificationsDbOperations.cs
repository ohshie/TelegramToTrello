using TelegramToTrello;

public class NotificationsDbOperations
{
    public async Task<TaskNotification> AddTaskNotification(RegisteredUsers user, TrelloOperations.TrelloCards card)
    {
        using BotDbContext dbContext = new BotDbContext();
        {
            TaskNotification notification = await dbContext.TaskNotifications.FindAsync(card.Id);
            if (notification == null)
            {
                dbContext.TaskNotifications.Add(new TaskNotification()
                {
                    Id = card.Id,
                    Name = card.Name,
                    Due = card.Due,
                    Url = card.Url,
                    User = user.TelegramId
                });
                await dbContext.SaveChangesAsync();
                return notification;
            }

            return notification;

        }
    }
}