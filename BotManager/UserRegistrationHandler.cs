using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;

namespace TelegramToTrello.UserRegistration;

public class UserRegistrationHandler
{
    public UserRegistrationHandler( 
        ITelegramBotClient botClient, 
        SyncService syncService, 
        UserDbOperations userDbOperations, MenuKeyboards menuKeyboards)
    {
        _botClient = botClient;
        _syncService = syncService;
        _userDbOperations = userDbOperations;
        _menuKeyboards = menuKeyboards;
    }
    
    private readonly ITelegramBotClient _botClient;
    private readonly SyncService _syncService;
    private readonly UserDbOperations _userDbOperations;
    private readonly MenuKeyboards _menuKeyboards;
    
    public async Task Authenticate(Message message)
    {
        string oauthLink = AuthLink.CreateLink(message.From!.Id);
        
        bool registerSuccess = await _userDbOperations.RegisterNewUser(message);
        if (!registerSuccess)
        {
            await _botClient.SendTextMessageAsync(message.Chat.Id,
                replyToMessageId: message.MessageId,
                text: "User already registered.",
                replyMarkup: _menuKeyboards.MainKeyboard());
            return;
        }
        
        await _botClient.SendTextMessageAsync(message.Chat.Id,
            replyToMessageId: message.MessageId,
            text: "Please click on this link authenticate in trello:\n\n" +
                  $"{oauthLink}\n\n",
            replyMarkup: _menuKeyboards.MainKeyboard());
    }
    
    public async Task SyncBoards(Message message)
    {
        RegisteredUser? user = await _userDbOperations.RetrieveTrelloUser((int)message.From!.Id);
        bool success = await _syncService.SyncStateToTrello(user);
        if (success)
        {
            await _botClient.SendTextMessageAsync(message.Chat.Id,text: "All set, you can now create tasks with /newtask");
            return;
        }

        await _botClient.SendTextMessageAsync(message.Chat.Id, 
            replyToMessageId: message.MessageId,
            text: "Looks like you haven't completed authentication via trello.\n" + 
                  "Click /register and finish authorization via trello website.");
    }
}