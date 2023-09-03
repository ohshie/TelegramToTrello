using Telegram.Bot.Types;

namespace TelegramToTrello.BotManager;

public class Verifier
{
    private readonly UserDbOperations _userDbOperations;
    private readonly TaskDbOperations _taskDbOperations;
    private readonly TemplatesDbOperations _templatesDbOperations;
    private readonly BotMessenger _botMessenger;

    public Verifier(UserDbOperations userDbOperations, TaskDbOperations taskDbOperations,
        TemplatesDbOperations templatesDbOperations, BotMessenger botMessenger)
    {
        _userDbOperations = userDbOperations;
        _taskDbOperations = taskDbOperations;
        _templatesDbOperations = templatesDbOperations;
        _botMessenger = botMessenger;
    }

    public async Task<User> GetUser(Message message)
    {
        User trelloUser = await _userDbOperations.RetrieveTrelloUser((int)message.Chat.Id);
        if (trelloUser is null)
        {
            await _botMessenger.SendMessage(chatId: (int)message.From.Id,
                text: "Looks like you are not registered yet." +
                      "Click on /register and follow commands to register");
        }
        
        return trelloUser;
    }

    public async Task<bool> CheckUser(int userId)
    {
        return await _userDbOperations.CheckIfExist(userId);
    }

    public async Task<bool> CheckTask(int userId)
    {
        return await _taskDbOperations.CheckIfExist(userId);
    }

    public async Task<bool> CheckTemplate(int userId)
    {
        return await _templatesDbOperations.CheckIfIncomplete(userId);
    }
    
    public async Task<Template> GetTemplate(int id, bool creationStart = false)
    {
        Template template = await _templatesDbOperations.GetIncompleteTemplate(id);

        if (template ==  null && !creationStart)
        {
            await _botMessenger.SendMessage(chatId: id,
                text: "Lets not get ahead of ourselves.\n" +
                    "Click on new template to start template creation process");
        }

        return template;
    }
}