using Telegram.Bot;
using TelegramToTrello.BotManager;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations.RequestFromUser;

public class RequestDesc : TemplateCreationBaseHandler
{
    private readonly BotMessenger _botMessenger;

    public RequestDesc(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TemplatesDbOperations templateDbOperations, Verifier verifier, BotMessenger botMessenger) : base(botClient, userDbOperations,
        templateDbOperations, verifier)
    {
        _botMessenger = botMessenger;
    }

    protected override async Task HandleTask(Template template)
    {
        await TemplateDbOperations.AddPlaceholderDesc(template);

        await _botMessenger.RemoveMessage(chatId: template.UserId, messageId: Message.MessageId);
        await _botMessenger.SendMessage(text: "Now please type description of your templated task in the next message.",
            chatId: template.UserId);
    }
}