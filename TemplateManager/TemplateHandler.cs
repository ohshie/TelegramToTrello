using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramToTrello.BotManager;

namespace TelegramToTrello.TemplateManager;


// how should template look?
// id, template name, Board name, board id, list id, Task name, Task description
// then it should prompt user to select participant, then date and go to push task menu
// hard things - selecting participants?
// template should also be a separate db table that many to one relationship with users.
// i guess that's it?

public class TemplateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly MenuKeyboards _menuKeyboards;
    private readonly TemplatesDbOperations _templatesDbOperations;
    private readonly BotMessenger _botMessenger;

    public TemplateHandler(ITelegramBotClient botClient, 
        MenuKeyboards menuKeyboards, 
        TemplatesDbOperations templatesDbOperations, 
        BotMessenger botMessenger)
    {
        _botClient = botClient;
        _menuKeyboards = menuKeyboards;
        _templatesDbOperations = templatesDbOperations;
        _botMessenger = botMessenger;
    }
    
    public async Task Display(Message message)
    {
        if (message.MessageId != null)
        {
            await _botMessenger.RemoveMessage((int)message.Chat.Id, message.MessageId);
        }
        
        await _botMessenger.SendMessage(chatId: (int)message.Chat.Id,
            text: await TemplatesMessage(userId: (int)message.From.Id),
            replyKeyboardMarkup:_menuKeyboards.TemplatesKeyboard());
    }

    private async Task<string> TemplatesMessage(int userId)
    {
        var templates = await _templatesDbOperations.ListTemplates(userId);

        if (templates == null || templates.Count == 0) return "You have no templates yet, create one by clicking button bellow.";
        int idConvert = 0;
        string text = "Here's your current templates: \n";
        foreach (var template in templates)
        {
           ++idConvert;
           text += $"{idConvert}. {template.TemplateName} on {template.BoardName}\n";
        }

        return text;
    }
}