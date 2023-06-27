using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello;

public class NotificationsRepository : INotificationsRepository
{
    public async Task<TaskNotification> Get(int id)
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.TaskNotifications.FindAsync(id);
        }
    }

    public async Task<TaskNotification> Get(string id)
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.TaskNotifications.FirstOrDefaultAsync(tn => tn.TaskId == id);
        }
    }

    public async Task<IEnumerable<TaskNotification>> GetAll()
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.TaskNotifications.ToListAsync();
        }
    }

    public async Task Add(TaskNotification entity)
    {
        using (BotDbContext dbContext = new())
        {
            dbContext.TaskNotifications.Add(entity);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task Update(TaskNotification entity)
    {
        using (BotDbContext dbContext = new())
        {
            dbContext.TaskNotifications.Update(entity);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task Delete(TaskNotification entity)
    {
        using (BotDbContext dbContext = new())
        {
            dbContext.TaskNotifications.Remove(entity);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task DeleteRange(List<TaskNotification> taskNotifications)
    {
        using (BotDbContext dbContext = new())
        {
            dbContext.TaskNotifications.RemoveRange(taskNotifications);
            await dbContext.SaveChangesAsync();
        }
    }

    public List<TaskNotification> GetAllPendingNotificationsByUserId(int id)
    {
        using (BotDbContext dbContext = new())
        {
            return dbContext.TaskNotifications
                    .Where(tn => tn.User == id).ToList();
        }
    }

    public async Task AddRange(List<TaskNotification> taskNotifications)
    {
        using (BotDbContext dbContext = new())
        {
            await dbContext.AddRangeAsync(taskNotifications);
            await dbContext.SaveChangesAsync();
        }
    }
}