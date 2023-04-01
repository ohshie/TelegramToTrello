using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
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
        
        var me = await _botClient.GetMeAsync();
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
        var userId = (int)message.Chat.Id;

        Console.WriteLine($"Received a '{messageText}' message in chat {chatId} from {userUsername}.");

        BotTaskCreation botTaskCreation = new BotTaskCreation(botClient);
        BotTaskAdditions botTaskAdditions = new BotTaskAdditions(botClient);
        
        if (message.Text.StartsWith("/register")) await RegisterUser(message, botClient);

        if (message.Text.StartsWith("/newtask")) await botTaskCreation.AddNewTaskToDb(message);
        
        if (message.Text.StartsWith("/board")) await botTaskCreation.NewTaskBoardSelection(message);

        if (message.Text.StartsWith("/list")) await botTaskCreation.NewTaskTableSelection(message);
        
        if (message.Text.StartsWith("/push")) await botTaskCreation.PushTaskToTrello(message);

        if (message.Text.StartsWith("/desc")) await botTaskAdditions.ChoosingATaskToAddAdittions(message);
        if (message.Text.StartsWith("/part")) await botTaskAdditions.ChoosingATaskToAddAdittions(message);
        if (message.Text.StartsWith("/name")) await botTaskAdditions.ChoosingATaskToAddAdittions(message);

    }
    
    private async Task RegisterUser(Message? message, ITelegramBotClient botClient)
    {
        string trelloUserName = message.Text.Substring("/register".Length).Trim();
        Console.WriteLine(trelloUserName);

        string trelloID = await _trelloOperations.GetTrelloUserIdFromTrelloAPI(trelloUserName);
        if (trelloID == null)
        {
            await _botClient.SendTextMessageAsync(message.Chat.Id, $"{trelloUserName} not found in trello.");
            return;
        }
        
        bool success = await _dbOperation.LinkTelegramToTrello(message, botClient, trelloID, trelloUserName);
        if (success) await botClient.SendTextMessageAsync(message.Chat.Id, "Trello account linked to telegram account.");
        
        success = await _dbOperation.LinkBoardsFromTrello((int)message.From.Id);
        if (success) await botClient.SendTextMessageAsync(message.Chat.Id, "Boards fetched");
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