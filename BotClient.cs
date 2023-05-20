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
    private static Timer Timer;
    private static readonly string? TelegramBotToken = Environment.GetEnvironmentVariable("Telegram_Bot_Token");
    private static readonly int TasksUpdateTimer = int.Parse(Environment.GetEnvironmentVariable("TaskUpdateTimer"));
    
    private TelegramBotClient _botClient = new(TelegramBotToken);
    private DbOperations _dbOperation = new();
    
    public async Task BotOperations()
    {
        using CancellationTokenSource cts = new CancellationTokenSource();

        ReceiverOptions receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };
        
        BotNotificationCentre botNotificationCentre = new BotNotificationCentre(_botClient);
        NotificationService(botNotificationCentre);
        
        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token);
        
        var me = await _botClient.GetMeAsync(cancellationToken:cts.Token);
        Console.WriteLine(_botClient.Timeout);
        Console.WriteLine($"Listening for @{me.Username}");
        Console.ReadLine();
        
        cts.Cancel();
    }

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        botClient.GetUpdatesAsync();
        Console.WriteLine(update.CallbackQuery?.Data);
        
        if (update.CallbackQuery is {} callbackQuery)
        {
            await CallBackDataManager(callbackQuery, botClient);
            return;
        }
        
        if (update.Message is not {} message) return;
        if (message.Chat.Id != message.From.Id) return;
        if (message.Text is not {} messageText) return;

        var chatId = message.Chat.Id;
        var userUsername = message.From?.Username;

        Console.WriteLine($"Received a '{messageText}' message in chat {chatId} from {userUsername}.");
        
        BotNotificationCentre botNotificationCentre = new BotNotificationCentre(message, botClient);

        TaskPlaceholderOperator taskPlaceholderOperator = new();
        {
            await taskPlaceholderOperator.SortMessage(message, botClient);
        }
        
        if (message.Text.StartsWith("/register")
            || message.Text.StartsWith("/start")) await Authenticate(message, botClient);
        if (message.Text.StartsWith("/CompleteRegistration")) await FinishAuth(message, botClient);

        if (message.Text.StartsWith("/newtask"))
        {
            StartTaskCreation startTaskCreation = new(message, botClient);
            await startTaskCreation.CreateTask();
        }
        
        if (message.Text.StartsWith("/notifications"))
            await botNotificationCentre.ToggleNotificationsForUser();
    }

    private async Task CallBackDataManager(CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        if (callbackQuery.Data.StartsWith("/board"))
        {
            AddBoardToTask addBoardToTask = new(callbackQuery, botClient);
            await addBoardToTask.Execute();
        }

        if (callbackQuery.Data.StartsWith("/list"))
        {
            AddTableToTask addTableToTask = new(callbackQuery, botClient);
            await addTableToTask.Execute();
        }

        if (callbackQuery.Data.StartsWith("/tag"))
        {
            AddTagToTask addTagToTask = new(callbackQuery, botClient);
            await addTagToTask.Execute();
        }

        if (callbackQuery.Data.StartsWith("/name"))
        {
            AddParticipantToTask addParticipantToTask = new(callbackQuery, botClient);
            await addParticipantToTask.Execute();
        }

        if (callbackQuery.Data.StartsWith("/push"))
        {
            PushTask pushTask = new(callbackQuery, botClient);
            await pushTask.Execute();
        }
    }
    
    private async Task Authenticate(Message message, ITelegramBotClient botClient)
    {
        string oauthLink = AuthLink.CreateLink(message.From!.Id);

        bool registerSuccess = await _dbOperation.RegisterNewUser(message);
        if (!registerSuccess)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,
                replyToMessageId: message.MessageId,
                text: "User already registered.");
            return;
        }
        
        await botClient.SendTextMessageAsync(message.Chat.Id,
            replyToMessageId: message.MessageId,
            text: "Please click on this link authenticate in trello:\n\n" +
                  $"{oauthLink}\n\n");
    }
    
    private async Task FinishAuth(Message message, ITelegramBotClient botClient)
    {
        bool success = await _dbOperation.LinkBoardsFromTrello((int)message.From!.Id);
        if (success)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,text: "All set, you can now create tasks with /newtask");
            return;
        }

        await botClient.SendTextMessageAsync(message.Chat.Id, 
            replyToMessageId: message.MessageId,
            text: "Looks like you haven't completed authentication via trello.\n" + 
                  "Click /register and finish authorization via trello website.");
    }

    private async Task NotificationService(BotNotificationCentre botNotificationCentre)
    {
        TimeSpan interval = TimeSpan.FromMinutes(TasksUpdateTimer);
        Timer = new Timer(async _ => await botNotificationCentre.NotificationManager(), null, interval, interval);
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