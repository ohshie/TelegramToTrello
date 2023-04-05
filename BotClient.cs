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
    private TrelloOperations _trelloOperations = new TrelloOperations();
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
        BotTaskAdditions botTaskAdditions = new BotTaskAdditions(botClient);
        
        if (message.Text.StartsWith("/register")) await RegisterUser(message, botClient);

        if (message.Text.StartsWith("/newtask")
            ||message.Text.StartsWith("/tag") 
            || message.Text.StartsWith("/board") 
            || message.Text.StartsWith("/list") 
            || message.Text.StartsWith("/push")) 
            await botTaskCreation.InitialTaskCreator(message);
        
        if (message.Text.StartsWith("/desc")
            || message.Text.StartsWith("/part") 
            || message.Text.StartsWith("/name")
            || message.Text.StartsWith("/date")
            || message.Text.StartsWith("/taskset"))
            await botTaskAdditions.ChoosingATaskToAddExtras(message);

        //if (message.Text.StartsWith("/listme")) await _dbOperation.SimpleList(message);
    }
    
    private async Task RegisterUser(Message? message, ITelegramBotClient botClient)
    {
        string trelloUserName = message.Text.Substring("/register".Length).Trim();
        Console.WriteLine(trelloUserName);
        if (trelloUserName.StartsWith("@teltotrelbot"))
        {
            await _botClient.SendTextMessageAsync(message.Chat.Id, 
                replyToMessageId: message.MessageId,
                text:$"Dont click on \"/register\"\n" +
                     $"Just type \"/register your trello username without @\"");
            return;
        }
        
        string trelloId = await _trelloOperations.GetTrelloUserIdFromTrelloApi(trelloUserName);
        if (trelloId == null)
        {
            await _botClient.SendTextMessageAsync(message.Chat.Id,
                replyToMessageId: message.MessageId,
                text:$"{trelloUserName} not found in trello or no username provided.");
            return;
        }
        
        bool success = await _dbOperation.LinkTelegramToTrello(message, botClient, trelloId, trelloUserName);
        if (success) await botClient.SendTextMessageAsync(message.Chat.Id,
            replyToMessageId: message.MessageId,
            text:"Trello account linked to telegram account.");
        
        success = await _dbOperation.LinkBoardsFromTrello((int)message.From.Id);
        if (success) await botClient.SendTextMessageAsync(message.Chat.Id,text: "Boards fetched");
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