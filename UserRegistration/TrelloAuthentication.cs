using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.UserRegistration;

public class TrelloAuthentication
{
    public TrelloAuthentication(Message message, ITelegramBotClient botClient)
    {
        Message = message;
        BotClient = botClient;
        DbOperations = new DbOperations();
    }

    private Message Message { get; set; }
    private ITelegramBotClient BotClient { get; set; }
    public DbOperations DbOperations { get; set; }
    
    public async Task Authenticate()
    {
        string oauthLink = AuthLink.CreateLink(Message.From!.Id);

        bool registerSuccess = await DbOperations.RegisterNewUser(Message);
        if (!registerSuccess)
        {
            await BotClient.SendTextMessageAsync(Message.Chat.Id,
                replyToMessageId: Message.MessageId,
                text: "User already registered.");
            return;
        }
        
        await BotClient.SendTextMessageAsync(Message.Chat.Id,
            replyToMessageId: Message.MessageId,
            text: "Please click on this link authenticate in trello:\n\n" +
                  $"{oauthLink}\n\n");
    }
    
    public async Task SyncBoards()
    {
        bool success = await DbOperations.LinkBoardsFromTrello((int)Message.From!.Id);
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
}