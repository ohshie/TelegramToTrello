using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramToTrello.BotActions;
using TelegramToTrello.CreatingTaskOperations;

namespace TelegramToTrello;

public class BotClient
{
    private static Timer NotificationsTimer;
    private static Timer SyncTimer;
    
    private static readonly string? TelegramBotToken = Environment.GetEnvironmentVariable("Telegram_Bot_Token");
    private static readonly int TasksUpdateTimer = int.Parse(Environment.GetEnvironmentVariable("TaskUpdateTimer"));
    private static readonly int SyncBoardsWithBot = int.Parse(Environment.GetEnvironmentVariable("SyncTimer"));

    private TelegramBotClient _botClient = new(TelegramBotToken);

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
        botClient.GetUpdatesAsync();
        
        if (update.CallbackQuery is { } callbackQuery)
        {
            TaskCallbackFactory callbackFactory = new();
            await callbackFactory.CallBackDataManager(callbackQuery, botClient);
            return;
        }

        if (update.Message is not { } message) return;
        if (message.Chat.Id != message.From.Id) return;
        if (message.Text is not { } messageText) return;
        
        var chatId = message.Chat.Id;
        var userUsername = message.From?.Username;

        Console.WriteLine($"Received a '{messageText}' message in chat {chatId} from {userUsername}.");
        
        ActionsFactory actionsFactory = new();
        await actionsFactory.BotActionFactory(message, botClient);
        
        TaskPlaceholderOperator taskPlaceholderOperator = new();
        {
            await taskPlaceholderOperator.SortMessage(message, botClient);
        }
    }

    private async Task RunServices(ITelegramBotClient botClient)
    {
        TimeSpan taskUpdateInterval = TimeSpan.FromMinutes(TasksUpdateTimer);
        TimeSpan syncInterval = TimeSpan.FromMinutes(SyncBoardsWithBot);
        
        BotNotificationCentre botNotificationCentre = new(botClient);
        SyncService syncService = new();
        NotificationsTimer = new Timer(async _ => await botNotificationCentre.NotificationManager(), null, taskUpdateInterval, taskUpdateInterval);
        NotificationsTimer = new Timer(async _ => await syncService.SynchronizeDataToTrello(), null, syncInterval, syncInterval);
    }
    
    Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}