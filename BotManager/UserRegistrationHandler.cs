using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.Repositories;

namespace TelegramToTrello.UserRegistration;

public class UserRegistrationHandler
{
    public UserRegistrationHandler( 
        SyncService syncService, 
        UserDbOperations userDbOperations, MenuKeyboards menuKeyboards,
        DialogueStorageDbOperations dialogueStorageDbOperations, BotMessenger botMessenger)
    {
        _syncService = syncService;
        _userDbOperations = userDbOperations;
        _menuKeyboards = menuKeyboards;
        _dialogueStorageDbOperations = dialogueStorageDbOperations;
        _botMessenger = botMessenger;
    }

    private readonly SyncService _syncService;
    private readonly UserDbOperations _userDbOperations;
    private readonly MenuKeyboards _menuKeyboards;
    private readonly DialogueStorageDbOperations _dialogueStorageDbOperations;
    private readonly BotMessenger _botMessenger;

    public async Task Authenticate(Message message)
    {
        string oauthLink = AuthLink.CreateLink(message.From!.Id);
        
        bool registerSuccess = await _userDbOperations.RegisterNewUser(message);
        if (!registerSuccess)
        {
            await _dialogueStorageDbOperations.CreateDialogue((int)message.From.Id);
            await _botMessenger.SendMessage(chatId: (int)message.Chat.Id,
                text: "User already registered.", 
                replyKeyboardMarkup: _menuKeyboards.MainKeyboard());
            return;
        }
        
        await _dialogueStorageDbOperations.CreateDialogue((int)message.From.Id);
        await _botMessenger.SendMessage((int)message.Chat.Id,
            text: "Please click on this link authenticate in trello:\n\n" +
                  $"{oauthLink}\n\n",
            replyKeyboardMarkup: _menuKeyboards.MainKeyboard());
    }
    
    public async Task SyncBoards(Message message)
    {
        User? user = await _userDbOperations.RetrieveTrelloUser((int)message.From!.Id);
        bool success = await _syncService.SyncStateToTrello(user);
        if (success)
        {
            await _botMessenger.SendMessage((int)message.Chat.Id,
                text: "All set, you can now create tasks with /newtask");
            return;
        }

        await _botMessenger.SendMessage((int)message.Chat.Id, 
            text: "Looks like you haven't completed authentication via trello.\n" + 
                  "Click /register and finish authorization via trello website.");
    }
}