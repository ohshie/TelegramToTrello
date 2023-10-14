using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramToTrello.BotManager;
using TelegramToTrello.TaskManager;
using TelegramToTrello.TaskManager.CreatingTaskOperations;
using ILogger = Serilog.ILogger;

namespace TelegramToTrello;

public class BotClient
{
    public BotClient(ITelegramBotClient botClient, 
        ActionsFactory actionsFactory, 
        CallbackFactory callbackFactory, 
        PlaceholderOperator taskPlaceholderOperator,
        IHost host, ILogger<BotClient> logger)
    {
        _botClient = botClient;
        _actionsFactory = actionsFactory;
        _callbackFactory = callbackFactory;
        _taskPlaceholderOperator = taskPlaceholderOperator;
        _host = host;
        _logger = logger;
    }

    private readonly ITelegramBotClient _botClient;
    private readonly ActionsFactory _actionsFactory;
    private readonly CallbackFactory _callbackFactory;
    private readonly PlaceholderOperator _taskPlaceholderOperator;
    private readonly IHost _host;
    private readonly ILogger<BotClient> _logger;

    public async Task BotOperations()
    {
        using CancellationTokenSource cts = new CancellationTokenSource();

        ReceiverOptions receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token);

        var me = await _botClient.GetMeAsync(cancellationToken: cts.Token);
        
        _logger.LogWarning("bot started @{Me}", me);
        
        Console.ReadLine();
        
        cts.Cancel();
    }

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        botClient.GetUpdatesAsync();
        
        using (var scope = _host.Services.CreateScope())
        {
            if (update.CallbackQuery is { } callbackQuery)
            {
                await _callbackFactory.CallBackDataManager(callbackQuery);
                return;
            }
            
            if (update.Message is not { } message) return;
            
            var chatId = message.Chat.Id;
            var userUsername = message.From?.Username;
            
            _logger.LogWarning("Recieved a {message} in chat {chatId} from {userUsername}", 
                message.Type, chatId, userUsername);
            
            await _actionsFactory.BotActionFactory(message);
            await _taskPlaceholderOperator.SortMessage(message);
        }
    }

    Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        try
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
        catch (Exception ex)
        {
            Console.WriteLine($"Error while processing update: {ex}");
            return Task.CompletedTask;
        }
    }
}