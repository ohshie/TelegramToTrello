using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.BotManager;

public class Verifier
{
    private readonly UserDbOperations _userDbOperations;
    private readonly TaskDbOperations _taskDbOperations;
    private readonly ITelegramBotClient _botClient;
    private TemplatesDbOperations _templatesDbOperations;

    public Verifier(UserDbOperations userDbOperations, ITelegramBotClient botClient, TaskDbOperations taskDbOperations, TemplatesDbOperations templatesDbOperations)
    {
        _userDbOperations = userDbOperations;
        _botClient = botClient;
        _taskDbOperations = taskDbOperations;
        _templatesDbOperations = templatesDbOperations;
    }

    public async Task<RegisteredUser> GetUser(Message message)
    {
        RegisteredUser trelloUser = await _userDbOperations.RetrieveTrelloUser((int)message.Chat.Id);
        if (trelloUser is null)
        {
            await _botClient.SendTextMessageAsync(chatId: message.From.Id,
                text: "Looks like you are not registered yet." +
                      "Click on /register and follow commands to register");
        }
        
        return trelloUser;
    }

    public async Task<TTTTask> GetTask(Message message, bool creationStart = false)
    {
        TTTTask userTask = await _taskDbOperations.RetrieveUserTask((int)message.Chat.Id);

        if (userTask == null && !creationStart)
        {
            await _botClient.SendTextMessageAsync(chatId: message.From.Id,
                text: "Lets not get ahead of ourselves.\n" +
                      "Click on new task to start task creation process");
        }
        
        return userTask;
    }

    public async Task<Template> GetTemplate(int id, bool creationStart = false)
    {
        Template template = await _templatesDbOperations.GetIncompleteTemplate(id);

        if (template ==  null && !creationStart)
        {
            await _botClient.SendTextMessageAsync(chatId: id,
                text: "Lets not get ahead of ourselves.\n" +
                    "Click on new template to start template creation process");
        }

        return template;
    }
}