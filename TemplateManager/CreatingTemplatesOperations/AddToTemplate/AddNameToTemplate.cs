using Telegram.Bot;
using TelegramToTrello.BotManager;
using TelegramToTrello.TemplateManager.CreatingTemplatesOperations.RequestFromUser;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations.AddToTemplate;

public class AddNameToTemplate : TemplateCreationBaseHandler
{
    private readonly RequestDesc _requestDesc;
    private readonly MessageRemover _messageRemover;

    public AddNameToTemplate(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TemplatesDbOperations templateDbOperations, Verifier verifier, RequestDesc requestDesc, MessageRemover messageRemover) : base(botClient, userDbOperations,
        templateDbOperations, verifier)
    {
        _requestDesc = requestDesc;
        _messageRemover = messageRemover;
    }

    protected override async Task HandleTask(RegisteredUser user, Template template)
    {
        string templateName = Message.Text;
        
        await _messageRemover.Remove(Message.Chat.Id, (int)Message.From.Id);
        await TemplateDbOperations.AddName(template, templateName);

        NextTask = _requestDesc;
    }
}