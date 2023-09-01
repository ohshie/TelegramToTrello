using Telegram.Bot;
using TelegramToTrello.BotManager;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations.RequestFromUser;

public class RequestName : TemplateCreationBaseHandler
{
    public RequestName(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TemplatesDbOperations templateDbOperations, Verifier verifier) : base(botClient, userDbOperations,
        templateDbOperations, verifier)
    {
    }

    protected override async Task HandleTask(User user, Template template)
    {
        await TemplateDbOperations.AddPlaceholderName(template);
        
        await BotClient.SendTextMessageAsync(text: "Now please type name of your templated task in the next message.",
            chatId: user.TelegramId);
    }
}