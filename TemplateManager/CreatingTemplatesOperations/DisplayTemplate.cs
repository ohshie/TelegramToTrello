using Telegram.Bot;
using TelegramToTrello.BotManager;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations;

public class DisplayTemplate : TemplateCreationBaseHandler
{
    private readonly ConfirmTemplateKeyboard _confirmTemplateKeyboard;

    public DisplayTemplate(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TemplatesDbOperations templateDbOperations, Verifier verifier,
        ConfirmTemplateKeyboard confirmTemplateKeyboard) : base(botClient, userDbOperations, templateDbOperations,
        verifier)
    {
        _confirmTemplateKeyboard = confirmTemplateKeyboard;
    }

    protected override async Task HandleTask(User user, Template template)
    {
        var replyMarkup = _confirmTemplateKeyboard.AssembleKeyboard();
        
        await BotClient.SendTextMessageAsync(text: "Lets review current template:\n\n" +
                                                   $"Template name: {template.TemplateName}\n" +
                                                   $"On board: {template.BoardName}\n"+
                                                   $"Description: {template.TaskDesc}\n"+
                                                   $"If everything is correct press save to save this template\n", 
            chatId: user.TelegramId, replyMarkup: replyMarkup);
    }
}