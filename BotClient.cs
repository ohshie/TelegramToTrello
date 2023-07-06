using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramToTrello.TaskManager;
using TelegramToTrello.TaskManager.CreatingTaskOperations;

namespace TelegramToTrello;

public class BotClient
{
    public BotClient(ITelegramBotClient botClient, 
        ActionsFactory actionsFactory, 
        CallbackFactory callbackFactory, 
        TaskPlaceholderOperator taskPlaceholderOperator,
        IHost host)
    {
        _botClient = botClient;
        _actionsFactory = actionsFactory;
        _callbackFactory = callbackFactory;
        _taskPlaceholderOperator = taskPlaceholderOperator;
        _host = host;
    }

    private readonly ITelegramBotClient _botClient;
    private readonly ActionsFactory _actionsFactory;
    private readonly CallbackFactory _callbackFactory;
    private readonly TaskPlaceholderOperator _taskPlaceholderOperator;
    private readonly IHost _host;

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
        Console.WriteLine(_botClient.Timeout);
        Console.WriteLine($"Listening for @{me.Username}");
        Console.ReadLine();
        
        cts.Cancel();
    }

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        botClient.GetUpdatesAsync();
        
        using (var scope = _host.Services.CreateScope())
        {
#pragma warning disable
            if (update.CallbackQuery is { } callbackQuery)
            {
                await _callbackFactory.CallBackDataManager(callbackQuery);
                return;
            }
            
            if (update.Message is not { } message) return;
            
            var chatId = message.Chat.Id;
            var userUsername = message.From?.Username;
            
            Console.WriteLine($"Received a '{message}' message in chat {chatId} from {userUsername}.");
            
            await _actionsFactory.BotActionFactory(message);
            await _taskPlaceholderOperator.SortMessage(message);
        }
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