using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello;

public class NotificationsRepository : INotificationsRepository
{
    private readonly BotDbContext _botDbContext;

    public NotificationsRepository(BotDbContext botDbContext)
    {
        _botDbContext = botDbContext;
    }

    public async Task<TaskNotification> Get(int id)
    {
        return await _botDbContext.TaskNotifications.FindAsync(id);
    }

    public async Task<TaskNotification> Get(string id)
    {
        return await _botDbContext.TaskNotifications.FirstOrDefaultAsync(tn => tn.TaskId == id);
    }

    public async Task<IEnumerable<TaskNotification>> GetAll()
    {
        return await _botDbContext.TaskNotifications.ToListAsync();
    }

    public async Task Add(TaskNotification entity)
    {
        _botDbContext.TaskNotifications.Add(entity);
        await _botDbContext.SaveChangesAsync();
    }

    public async Task Update(TaskNotification entity)
    {
        _botDbContext.TaskNotifications.Update(entity);
        await _botDbContext.SaveChangesAsync();
    }

    public async Task Delete(TaskNotification entity)
    {
        _botDbContext.TaskNotifications.Remove(entity);
        await _botDbContext.SaveChangesAsync();
    }

    public async Task DeleteRange(List<TaskNotification> taskNotifications)
    {
        _botDbContext.TaskNotifications.RemoveRange(taskNotifications);
        await _botDbContext.SaveChangesAsync();
    }

    public List<TaskNotification> GetAllPendingNotificationsByUserId(int id)
    {
        
        return _botDbContext.TaskNotifications
                    .Where(tn => tn.User == id).ToList();
    }

    public async Task AddRange(List<TaskNotification> taskNotifications)
    {
        await _botDbContext.AddRangeAsync(taskNotifications);
        await _botDbContext.SaveChangesAsync();
    }
}