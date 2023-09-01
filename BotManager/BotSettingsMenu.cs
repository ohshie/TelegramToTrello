using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.BotManager;

public class BotSettingsMenu
{
    private readonly ITelegramBotClient _botClient;
    private readonly UserDbOperations _userDbOperations;
    private readonly MenuKeyboards _menuKeyboards;
    private int _chatId;
    private int _messageId;
    private readonly BotMessenger _botMessenger;

    public BotSettingsMenu(ITelegramBotClient botClient,
        UserDbOperations userDbOperations, MenuKeyboards menuKeyboards, BotMessenger botMessenger)
    {
        _botClient = botClient;
        _userDbOperations = userDbOperations;
        _menuKeyboards = menuKeyboards;
        _botMessenger = botMessenger;
    }

    public async Task Display(Message message)
    {
        _chatId = (int)message.Chat.Id;
        _messageId = message.MessageId;
        
        if (!await CheckIfRegistered(message)) return;

        await _botMessenger.RemoveMessage(_chatId, _messageId);

        await _botClient.SendTextMessageAsync(chatId: _chatId,
            text: "Choose menu item from a keyboard bellow.", replyMarkup: _menuKeyboards.SettingsKeyboard());
    }

    private async Task<bool> CheckIfRegistered(Message message)
    {
        if (message.From.Id == null) return false;
        var user = await _userDbOperations.RetrieveTrelloUser((int)message.From.Id);
        if (user == null)
        {
            await _botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: "Looks like you haven't registered yet. Type /register first and follow registration process");

            return false;
        }

        return true;
    }

    public async Task CloseMenu(Message message)
    {
        _chatId = (int)message.Chat.Id;
        _messageId = message.MessageId;
        
        if (!await CheckIfRegistered(message)) return;

        await _botMessenger.RemoveMessage(_chatId, _messageId);
        
        await _botClient.SendTextMessageAsync(chatId: _chatId, text: "Back to work.",
            replyMarkup: _menuKeyboards.MainKeyboard());
    }
}