using Telegram.Bot;
using TelegramToTrello.BotManager;
using TelegramToTrello.TemplateManager.CreatingTemplatesOperations.RequestFromUser;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations.AddToTemplate;

public class AddNameToTemplate : TemplateCreationBaseHandler
{
    private readonly RequestDesc _requestDesc;
    private readonly BotMessenger _botMessenger;

    public AddNameToTemplate(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TemplatesDbOperations templateDbOperations, Verifier verifier, RequestDesc requestDesc, BotMessenger botMessenger) : base(botClient, userDbOperations,
        templateDbOperations, verifier)
    {
        _requestDesc = requestDesc;
        _botMessenger = botMessenger;
    }

    protected override async Task HandleTask(Template template)
    {
        string templateName = Message.Text;
        
        await _botMessenger.RemoveMessage(chatId: template.UserId, Message.MessageId);
        await TemplateDbOperations.AddName(template, templateName);

        NextTask = _requestDesc;
    }
}