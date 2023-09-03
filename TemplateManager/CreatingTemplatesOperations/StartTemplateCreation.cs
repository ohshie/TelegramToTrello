using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.TemplateManager.CreatingTemplatesOperations.CreateKeyboards;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations;

public class StartTemplateCreation
{
    private readonly TemplateCreateKbWithBoards _createKeyboardWithBoards;
    private readonly Verifier _verifier;
    private readonly BotMessenger _botMessenger;
    private readonly TemplatesDbOperations _templatesDbOperations;
    
    public StartTemplateCreation(TemplatesDbOperations templatesDbOperations, 
        TemplateCreateKbWithBoards createKeyboardWithBoards,
        Verifier verifier,
        BotMessenger botMessenger)
    {
        _templatesDbOperations = templatesDbOperations;
        _createKeyboardWithBoards = createKeyboardWithBoards;
        _verifier = verifier;
        _botMessenger = botMessenger;
    }

    public async Task CreateTemplate(Message message)
    {
        if (!await _verifier.CheckUser((int)message.From.Id))
        {
            await _botMessenger.RemoveMessage((int)message.Chat.Id, message.MessageId);
            return;
        }
        
        Template template = await _verifier.GetTemplate((int)message.From.Id, creationStart: true);
        if (template is not null)
        {
            await _botMessenger.RemoveMessage((int)message.Chat.Id, message.MessageId);
            return;
        }

        await _templatesDbOperations.StartTemplate((int)message.From.Id);

        await _botMessenger.RemoveMessage((int)message.Chat.Id, message.MessageId);
        await _createKeyboardWithBoards.Execute(message);
    }
}