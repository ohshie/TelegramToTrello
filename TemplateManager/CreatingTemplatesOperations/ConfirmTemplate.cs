using Telegram.Bot;
using TelegramToTrello.BotManager;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations;

public class ConfirmTemplate : TemplateCreationBaseHandler
{
    public ConfirmTemplate(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TemplatesDbOperations templateDbOperations, Verifier verifier) : base(botClient, userDbOperations,
        templateDbOperations, verifier)
    {
    }

    protected override async Task HandleTask(RegisteredUser user, Template template)
    {
        await TemplateDbOperations.SaveTemplate(template);

        await BotClient.SendTextMessageAsync(chatId: user.TelegramId,
            text: "Template successfully saved");
    }
}