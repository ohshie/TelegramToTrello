using Open.Linq.AsyncExtensions;
using TelegramToTrello.ToFromTrello;

namespace TelegramToTrello;

public class NotificationsDbOperations
{
    private readonly INotificationsRepository _notificationsRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IRepository<Board> _boardRepository;

    public NotificationsDbOperations(INotificationsRepository notificationsRepository, 
        IUsersRepository usersRepository, 
        ITableRepository tableRepository, 
        IRepository<Board> boardRepository)
    {
        _notificationsRepository = notificationsRepository;
        _usersRepository = usersRepository;
        _tableRepository = tableRepository;
        _boardRepository = boardRepository;
    }

    public async Task<User> ToggleNotifications(int telegramId)
    {
        var user = await _usersRepository.GetUserWithBoards(telegramId);
        
        if (user != null)
        {
            user.NotificationsEnabled = !user.NotificationsEnabled;
            await _usersRepository.Update(user);
            return user;
        }

        return null;
    }
    
    public async Task<List<TaskNotification>> UpdateAndAddCards(User user, Dictionary<string, TrelloOperations.TrelloCard> cards)
    {
        List<TaskNotification> newTasks = new();
        
        await RemoveTasksThatAreNotInTrello(user, cards);

        var currentNotificationsMap = await _notificationsRepository.GetAll()
            .ToDictionary(tn => tn.TaskId);

        var newNotificationsKeys = cards.Keys.Except(currentNotificationsMap.Keys);
        var boards = await _boardRepository.GetAll();
        
        if (newNotificationsKeys.Any())
        {
            foreach (var key in newNotificationsKeys)
            {
                string correctDueDate = DateTime.Parse(cards[key].Due).AddHours(4).ToString("MM.dd.yyyy HH:mm");
                var board = boards.FirstOrDefault(b => b.TrelloBoardId == cards[key].BoardId);
                var table = board.Tables.FirstOrDefault(t => t.TableId == cards[key].ListId);

                TaskNotification newNotification = new()
                {
                    TaskId = cards[key].Id,
                    Name = cards[key].Name,
                    Due = correctDueDate,
                    Url = cards[key].Url,
                    User = user.TelegramId,
                    Description = cards[key].Description,
                    Participants = cards[key].Members,
                    TaskBoardId = cards[key].BoardId,
                    TaskListId = cards[key].ListId,
                    TaskBoard = board.BoardName,
                    TaskList = table.Name
                };
                newTasks.Add(newNotification);
            }

            await _notificationsRepository.AddRange(newTasks);
        }
        return newTasks;
    }

    public async Task<List<TaskNotification>> GetUserCardsFromDb(User trelloUser)
    {
        var taskList = _notificationsRepository.GetAllPendingNotificationsByUserId(trelloUser.TelegramId);
        return taskList;
    }

    private async Task RemoveTasksThatAreNotInTrello(User user, Dictionary<string, TrelloOperations.TrelloCard> cards)
    {
        var notifications = _notificationsRepository.GetAllPendingNotificationsByUserId(user.TelegramId);
        List<string> cardsIds = cards.Values.Select(c => c.Id).ToList();
        
        notifications.RemoveAll(item => cardsIds.Contains(item.TaskId));
        await _notificationsRepository.DeleteRange(notifications);
    }

    public async Task<List<User>> GetUsersWithNotificationsEnabled()
    {
        var allUsers = await _usersRepository.GetAll();
        var usersWithNotificationsList = allUsers.Where(u => u.NotificationsEnabled)
            .ToList();

        return usersWithNotificationsList;
    }

    public async Task ToggleSentStatus(TaskNotification taskNotification)
    {
        taskNotification.NotificationSent = true;
        await _notificationsRepository.Update(taskNotification);
    }

    public async Task RemoveAssignedTask(TaskNotification task)
    {
        await _notificationsRepository.Delete(task);
    }
    
    public async Task<TaskNotification?> RetrieveAssignedTask(string taskId)
    {
        var task = await _notificationsRepository.Get(taskId);

        if (task != null)
        {
            return task;
        }
            
        return null;
    }
}