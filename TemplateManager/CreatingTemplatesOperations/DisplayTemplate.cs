using Telegram.Bot;
using TelegramToTrello.BotManager;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations;

public class DisplayTemplate : TemplateCreationBaseHandler
{
    private readonly ConfirmTemplateKeyboard _confirmTemplateKeyboard;
    private readonly BotMessenger _botMessenger;

    public DisplayTemplate(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TemplatesDbOperations templateDbOperations, Verifier verifier,
        ConfirmTemplateKeyboard confirmTemplateKeyboard, BotMessenger botMessenger) : base(botClient, userDbOperations, templateDbOperations,
        verifier)
    {
        _confirmTemplateKeyboard = confirmTemplateKeyboard;
        _botMessenger = botMessenger;
    }

    protected override async Task HandleTask(Template template)
    {
        var replyMarkup = _confirmTemplateKeyboard.AssembleKeyboard();
        
        await _botMessenger.SendMessage(text: "Lets review current template:\n\n" +
                                                   $"Template name: {template.TemplateName}\n" +
                                                   $"On board: {template.BoardName}\n"+
                                                   $"Description: {template.TaskDesc}\n"+
                                                   $"If everything is correct press save to save this template\n", 
            chatId: template.UserId, replyKeyboardMarkup: replyMarkup);
    }
}