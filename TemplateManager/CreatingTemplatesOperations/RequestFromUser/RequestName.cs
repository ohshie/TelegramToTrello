using Telegram.Bot;
using TelegramToTrello.BotManager;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations.RequestFromUser;

public class RequestName : TemplateCreationBaseHandler
{
    private readonly BotMessenger _botMessenger;

    public RequestName(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TemplatesDbOperations templateDbOperations, Verifier verifier, BotMessenger botMessenger) : base(botClient, userDbOperations,
        templateDbOperations, verifier)
    {
        _botMessenger = botMessenger;
    }

    protected override async Task HandleTask(Template template)
    {
        await TemplateDbOperations.AddPlaceholderName(template);

        await _botMessenger.RemoveLastBotMessage(template.UserId);
        await _botMessenger.SendMessage(text: "Now please type name of your templated task in the next message.",
            chatId: template.UserId);
    }
}