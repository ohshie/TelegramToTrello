using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramToTrello.BotActions;

namespace TelegramToTrello;

public class BotClient
{
    private static readonly string TelegramBotToken = Environment.GetEnvironmentVariable("Telegram_Bot_Token");
    
    private TelegramBotClient _botClient = new TelegramBotClient(TelegramBotToken);
    private DbOperations _dbOperation = new DbOperations();
    
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
        
        var me = await _botClient.GetMeAsync(cancellationToken:cts.Token);
        Console.WriteLine(_botClient.Timeout);
        Console.WriteLine($"Listening for @{me.Username}");
        Console.ReadLine();
        
        cts.Cancel();
    }

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        botClient.GetUpdatesAsync();
        
        if (update.Message is not {} message) return;
        if (message.Text is not {} messageText) return;

        var chatId = message.Chat.Id;
        var userUsername = message.From.Username;

        Console.WriteLine($"Received a '{messageText}' message in chat {chatId} from {userUsername}.");

        BotTaskCreation botTaskCreation = new BotTaskCreation(botClient);

        await MessagesToReplacePlaceholdersWithValues(message, botClient,botTaskCreation);
        
        if (message.Text.StartsWith("/register")) await Authenticate(message, botClient);
        if (message.Text.StartsWith("/CompleteRegistration")) await FinishAuth(message, botClient);

        if (message.Text.StartsWith("/newtask")
            || message.Text.StartsWith("/tag") 
            || message.Text.StartsWith("/board") 
            || message.Text.StartsWith("/list") 
            || message.Text.StartsWith("/push")
            || message.Text.StartsWith("/desc")
            || message.Text.StartsWith("/part")
            || message.Text.StartsWith("/name")
            || message.Text.StartsWith("/date"))
            await botTaskCreation.InitialTaskCreator(message);
    }

    private async Task Authenticate(Message? message, ITelegramBotClient botClient)
    {
        string oauthLink = AuthLink.CreateLink(message.From.Id);

        await _dbOperation.RegisterNewUser(message, botClient);
        
        await botClient.SendTextMessageAsync(message.Chat.Id,
            replyToMessageId: message.MessageId,
            text: "Please click on this link authenticate in trello:\n\n" +
                  $"{oauthLink}\n\n" +
                  "When done click /CompleteRegistration");
    }
    
    private async Task FinishAuth(Message message, ITelegramBotClient botClient)
    {
        bool success = await _dbOperation.LinkBoardsFromTrello((int)message.From.Id);
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

    public async Task MessagesToReplacePlaceholdersWithValues(Message message, ITelegramBotClient botClient, BotTaskCreation botTaskCreation)
    {
        using (BotDbContext dbContext = new BotDbContext())
        {
            TTTTask task = await dbContext.CreatingTasks.FindAsync((int)message.From.Id);
            if (task == null) return;
            
            if (task.TaskName == "###tempname###")
            {
                await botTaskCreation.AddNameToTask(task, message);
                return;
            }
            if (task.TaskDesc == "###tempdesc###")
            {
                await botTaskCreation.AddDescriptionToTask(task, message);
                return;
            }
            if (task.Date == "###tempdate###")
            {
                await botTaskCreation.AddDateToTask(task, message);
            }
        }
        
    //await _trelloOperations.PushTaskDescriptionToTrello(task);
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