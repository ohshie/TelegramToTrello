using Telegram.Bot;
using TelegramToTrello.BotManager;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations;

public class ConfirmTemplate : TemplateCreationBaseHandler
{
    private readonly BotMessenger _botMessenger;

    public ConfirmTemplate(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TemplatesDbOperations templateDbOperations, Verifier verifier, BotMessenger botMessenger) : base(botClient, userDbOperations,
        templateDbOperations, verifier)
    {
        _botMessenger = botMessenger;
    }

    protected override async Task HandleTask(Template template)
    {
        await TemplateDbOperations.SaveTemplate(template);

        await _botMessenger.SendMessage(chatId: template.UserId,
            text: "Template successfully saved");
    }
}