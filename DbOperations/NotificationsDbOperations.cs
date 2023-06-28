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

    public async Task<RegisteredUser> ToggleNotifications(int telegramId)
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
    
    public async Task<List<TaskNotification>> UpdateAndAddCards(RegisteredUser user, Dictionary<string, TrelloOperations.TrelloCard> cards)
    {
        List<TaskNotification> newTasks = new();
        
        await RemoveTasksThatAreNotInTrello(user, cards);

        var currentNotifications = await _notificationsRepository.GetAll();
        var currentNotificationsMap = currentNotifications
            .ToDictionary(tn => tn.TaskId);

        var newNotificationsKeys = cards.Keys.Except(currentNotificationsMap.Keys);

        if (newNotificationsKeys.Any())
        {
            foreach (var key in newNotificationsKeys)
            {
                string correctDueDate = DateTime.Parse(cards[key].Due).AddHours(4).ToString("MM.dd.yyyy HH:mm");
                Board? board = await _boardRepository.Get(cards[key].BoardId);
                Table? table = await _tableRepository.Get(cards[key].ListId);
                
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

    public async Task<List<TaskNotification>> GetUserCardsFromDb(RegisteredUser trelloUser)
    {
        var taskList = _notificationsRepository.GetAllPendingNotificationsByUserId(trelloUser.TelegramId);
        return taskList;
    }

    private async Task RemoveTasksThatAreNotInTrello(RegisteredUser user, Dictionary<string, TrelloOperations.TrelloCard> cards)
    {
        var notifications = _notificationsRepository.GetAllPendingNotificationsByUserId(user.TelegramId);
        List<string> cardsIds = cards.Values.Select(c => c.Id).ToList();
        
        notifications.RemoveAll(item => cardsIds.Contains(item.TaskId));
        await _notificationsRepository.DeleteRange(notifications);
    }

    public async Task<List<RegisteredUser>> GetUsersWithNotificationsEnabled()
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
}