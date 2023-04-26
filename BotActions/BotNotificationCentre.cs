using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.BotActions;

public class BotNotificationCentre
{
    private DbOperations _dbOperation = new DbOperations();
    private NotificationsDbOperations _notificationsDbOperations = new NotificationsDbOperations();
    
    private TrelloOperations _trelloOperations = new TrelloOperations();

    private ITelegramBotClient BotClient { get; }

    public BotNotificationCentre(ITelegramBotClient botClient)
    {
        BotClient = botClient;
    }

    public async Task NotificationExperiment(Message message)
    {
        RegisteredUsers user = await _dbOperation.RetrieveTrelloUser((int)message.From.Id);
        if (user == null) return;;

        List<Task<List<TrelloOperations.TrelloCards>>> tasks = new List<Task<List<TrelloOperations.TrelloCards>>>();
        foreach (var board in user.UsersBoards.Select(ub => ub.Boards))
        {
            tasks.Add(_trelloOperations.GetCardsOnBoards(board, user));
        }

        List<TrelloOperations.TrelloCards>[] results = await Task.WhenAll(tasks);

        DateTime now = DateTime.UtcNow;
        
        foreach (var cards in results)
        {
            foreach (var card in cards)
            {
                if (card.Members.Contains(user.TrelloId) && card.Due != null && DateTime.Parse(card.Due) > now && !card.Complete)
                {

                    TaskNotification notification = await _notificationsDbOperations.AddTaskNotification(user, card);
                    if (notification == null) continue;
                   
                    Console.WriteLine(notification.Id);
                    Console.WriteLine(notification.Name);
                    Console.WriteLine(notification.Due);
                    await BotClient.SendTextMessageAsync(text: $"you have a task: \"{notification.Name}\". Link {notification.Url}",
                        chatId: user.TelegramId);
                }
            }   
        }

    }
    
}