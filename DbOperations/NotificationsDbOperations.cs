using Microsoft.EntityFrameworkCore;
using TelegramToTrello.ToFromTrello;

namespace TelegramToTrello;

public class NotificationsDbOperations
{
    public async Task<RegisteredUser> ToggleNotifications(int telegramId)
    {
        using BotDbContext dbContext = new BotDbContext();
        {
            RegisteredUser? user = await dbContext.Users
                .Include(u => u.Boards)
                .FirstOrDefaultAsync(u => u.TelegramId == telegramId);
            if ( user!= null)
            {
                user.NotificationsEnabled = !user.NotificationsEnabled;
                dbContext.Update(user);
                await dbContext.SaveChangesAsync();
                return user;
            }

            return null;
        }
    }
    
    public async Task<List<TaskNotification>> UpdateAndAddCards(RegisteredUser user, Dictionary<string, TrelloOperations.TrelloCard> cards)
    {
        using (BotDbContext dbContext = new BotDbContext())
        {
            await RemoveTasksThatAreNotInTrello(dbContext, user, cards);
            
            List<TaskNotification> newTasks = new List<TaskNotification>();

            var currentNotifications = dbContext.TaskNotifications.ToDictionary(tn => tn.TaskId);
            var newNotificationsKeys = cards.Keys.Except(currentNotifications.Keys);
            
            if (newNotificationsKeys.Any())
            {
                foreach (var key in newNotificationsKeys)
                {
                    string correctDueDate = DateTime.Parse(cards[key].Due).AddHours(4).ToString("MM.dd.yyyy HH:mm");
                    Board? board = await dbContext.Boards.FirstOrDefaultAsync(b => b.TrelloBoardId == cards[key].BoardId);
                    Table? table = await dbContext.BoardTables.FirstOrDefaultAsync(t => t.TableId == cards[key].ListId);

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