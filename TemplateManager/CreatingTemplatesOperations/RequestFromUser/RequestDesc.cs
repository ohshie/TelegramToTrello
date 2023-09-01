using Telegram.Bot;
using TelegramToTrello.BotManager;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations.RequestFromUser;

public class RequestDesc : TemplateCreationBaseHandler
{
    public RequestDesc(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TemplatesDbOperations templateDbOperations, Verifier verifier) : base(botClient, userDbOperations,
        templateDbOperations, verifier)
    {
    }

    protected override async Task HandleTask(User user, Template template)
    {
        await TemplateDbOperations.AddPlaceholderDesc(template);
        
        await BotClient.SendTextMessageAsync(text: "Now please type description of your templated task in the next message.",
            chatId: user.TelegramId);
    }
}