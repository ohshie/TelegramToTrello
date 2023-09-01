using Telegram.Bot;
using TelegramToTrello.BotManager;
using TelegramToTrello.TemplateManager.CreatingTemplatesOperations.RequestFromUser;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations.AddToTemplate;

public class AddDescToTemplate : TemplateCreationBaseHandler
{
    private readonly DisplayTemplate _displayTemplate;
    private readonly BotMessenger _botMessenger;

    public AddDescToTemplate(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TemplatesDbOperations templateDbOperations, Verifier verifier, DisplayTemplate displayTemplate,
        BotMessenger botMessenger) : base(botClient, userDbOperations,
        templateDbOperations, verifier)
    {
        _displayTemplate = displayTemplate;
        _botMessenger = botMessenger;
    }

    protected override async Task HandleTask(User user, Template template)
    {
        string templateDesc = Message.Text;

        await _botMessenger.RemoveMessage(user.TelegramId, Message.MessageId);
        await TemplateDbOperations.AddDesc(template, templateDesc);

        NextTask = _displayTemplate;
    }
}