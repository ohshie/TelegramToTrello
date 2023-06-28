using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramToTrello.Notifications;
using TelegramToTrello.TaskManager;
using TelegramToTrello.TaskManager.CreatingTaskOperations;

namespace TelegramToTrello;

public class BotClient
{
    public BotClient(ITelegramBotClient botClient, 
        ActionsFactory actionsFactory, 
        CallbackFactory callbackFactory, 
        TaskPlaceholderOperator taskPlaceholderOperator, 
        BotNotificationCentre botNotificationCentre, 
        SyncService syncService)
    {
        _botClient = botClient;
        _actionsFactory = actionsFactory;
        _callbackFactory = callbackFactory;
        _taskPlaceholderOperator = taskPlaceholderOperator;
        _botNotificationCentre = botNotificationCentre;
        _syncService = syncService;

        _tasksUpdateTimer = Configuration.NotificationTimer;
        _syncBoardsWithBot = Configuration.SyncTimer;
    }
    
    private static Timer NotificationsTimer;
    private static Timer SyncTimer;
    
    private static int _tasksUpdateTimer;
    private static int _syncBoardsWithBot;
        
    private readonly ITelegramBotClient _botClient;
    private readonly ActionsFactory _actionsFactory;
    private readonly CallbackFactory _callbackFactory;
    private readonly TaskPlaceholderOperator _taskPlaceholderOperator;
    private readonly BotNotificationCentre _botNotificationCentre;
    private readonly SyncService _syncService;

    public async Task BotOperations()
    {
        using CancellationTokenSource cts = new CancellationTokenSource();

        ReceiverOptions receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };
        
        RunServices(_botClient);

        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token);

        var me = await _botClient.GetMeAsync(cancellationToken: cts.Token);
        Console.WriteLine(_botClient.Timeout);
        Console.WriteLine($"Listening for @{me.Username}");
        Console.ReadLine();

        cts.Cancel();
    }

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        using (var scope = Program.Container.BeginLifetimeScope())
        {
            #pragma warning disable
            botClient.GetUpdatesAsync();

            if (update.CallbackQuery is { } callbackQuery)
            {
                await _callbackFactory.CallBackDataManager(callbackQuery);
                return;
            }
            
            if (update.Message is not { } message) return;
            if (message.Text is not { } messageText) return;
            
            var chatId = message.Chat.Id;
            var userUsername = message.From?.Username;
        
            Console.WriteLine($"Received a '{messageText}' message in chat {chatId} from {userUsername}.");
            
            await _actionsFactory.BotActionFactory(message);
            await _taskPlaceholderOperator.SortMessage(message);
        }
    }

    private async Task RunServices(ITelegramBotClient botClient)
    {
        TimeSpan taskUpdateInterval = TimeSpan.FromMinutes(_tasksUpdateTimer);
        TimeSpan syncInterval = TimeSpan.FromMinutes(_syncBoardsWithBot);
        
        NotificationsTimer = new Timer(async _ => await _botNotificationCentre.NotificationManager(), null, taskUpdateInterval, taskUpdateInterval);
        SyncTimer = new Timer(async _ => await _syncService.SynchronizeDataToTrello(), null, syncInterval, syncInterval);
    }
    
    Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}