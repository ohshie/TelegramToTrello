using Telegram.Bot;
using TelegramToTrello.BotManager;
using TelegramToTrello.TemplateManager.CreatingTemplatesOperations.RequestFromUser;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations.AddToTemplate;

public class AddDescToTemplate : TemplateCreationBaseHandler
{
    private readonly DisplayTemplate _displayTemplate;
    private readonly MessageRemover _messageRemover;

    public AddDescToTemplate(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TemplatesDbOperations templateDbOperations, Verifier verifier, DisplayTemplate displayTemplate,
        MessageRemover messageRemover) : base(botClient, userDbOperations,
        templateDbOperations, verifier)
    {
        _displayTemplate = displayTemplate;
        _messageRemover = messageRemover;
    }

    protected override async Task HandleTask(RegisteredUser user, Template template)
    {
        string templateDesc = Message.Text;

        await _messageRemover.Remove(Message.Chat.Id, (int)Message.From.Id);
        await TemplateDbOperations.AddDesc(template, templateDesc);

        NextTask = _displayTemplate;
    }
}