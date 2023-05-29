using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.UserRegistration;

public class UserRegistrationHandler
{
    public UserRegistrationHandler(Message message, ITelegramBotClient botClient)
    {
        Message = message;
        BotClient = botClient;
        SyncService = new();
        UserDbOperations = new();
    }

    private Message Message { get; }
    private ITelegramBotClient BotClient { get; }
    private SyncService SyncService { get; }
    private UserDbOperations UserDbOperations { get; set; }
    
    public async Task Authenticate()
    {
        string oauthLink = AuthLink.CreateLink(Message.From!.Id);
        
        bool registerSuccess = await UserDbOperations.RegisterNewUser(Message);
        if (!registerSuccess)
        {
            await BotClient.SendTextMessageAsync(Message.Chat.Id,
                replyToMessageId: Message.MessageId,
                text: "User already registered.",
                replyMarkup: CreateKeyboard());
            return;
        }
        
        await BotClient.SendTextMessageAsync(Message.Chat.Id,
            replyToMessageId: Message.MessageId,
            text: "Please click on this link authenticate in trello:\n\n" +
                  $"{oauthLink}\n\n",
            replyMarkup:CreateKeyboard());
    }
    
    public async Task SyncBoards()
    {
        RegisteredUser? user = await UserDbOperations.RetrieveTrelloUser((int)Message.From!.Id);
        bool success = await SyncService.SyncStateToTrello(user);
        if (success)
        {
            await BotClient.SendTextMessageAsync(Message.Chat.Id,text: "All set, you can now create tasks with /newtask");
            return;
        }

        await BotClient.SendTextMessageAsync(Message.Chat.Id, 
            replyToMessageId: Message.MessageId,
            text: "Looks like you haven't completed authentication via trello.\n" + 
                  "Click /register and finish authorization via trello website.");
    }

    private ReplyKeyboardMarkup CreateKeyboard()
    {
        ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] {"➕New Task", "➖Cancel action"},
            new KeyboardButton[] {"♾️Show my tasks", "🟰Sync changes"}
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = false,
            IsPersistent = true
        };
        return keyboard;
    }
}