namespace TelegramToTrello;

public interface INotificationsRepository : IRepository<TaskNotification>
{
    Task DeleteRange(List<TaskNotification> taskNotificationsList);
    List<TaskNotification> GetAllPendingNotificationsByUserId(int id);
    Task AddRange(List<TaskNotification> taskNotifications);
}