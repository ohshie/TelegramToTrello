using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;
using TelegramToTrello.TaskManager.CreatingTaskOperations;
using TelegramToTrello.TemplateManager.CreatingTemplatesOperations.CreateKeyboards;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations;

public class StartTemplateCreation
{
    private Message? _message;
    private readonly TemplateCreateKbWithBoards _createKeyboardWithBoards;
    private readonly MessageRemover _messageRemover;
    private readonly Verifier _verifier;
    private readonly TemplatesDbOperations _templatesDbOperations;

    private ITelegramBotClient BotClient { get; }


    public StartTemplateCreation(ITelegramBotClient botClient, 
        TemplatesDbOperations templatesDbOperations, 
        TemplateCreateKbWithBoards createKeyboardWithBoards,
        MessageRemover messageRemover,
        Verifier verifier)
    {
        _templatesDbOperations = templatesDbOperations;
        _createKeyboardWithBoards = createKeyboardWithBoards;
        _messageRemover = messageRemover;
        _verifier = verifier;
        BotClient = botClient;
    }

    public async Task CreateTemplate(Message message)
    {
        _message = message;

        var user = await _verifier.GetUser(message);
        if (user is null)
        {
            await _messageRemover.Remove(_message.Chat.Id, _message.MessageId);
            return;
        }

        Template template = await _verifier.GetTemplate(user.TelegramId, creationStart: true);
        if (template is not null)
        {
            await _messageRemover.Remove(_message.Chat.Id, _message.MessageId);
            return;
        }

        await _templatesDbOperations.StartTemplate(user);

        await _messageRemover.Remove(_message.Chat.Id, _message.MessageId);
        await _createKeyboardWithBoards.Execute(_message);
    }
}