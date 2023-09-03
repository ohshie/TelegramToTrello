using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.BotManager;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations.CreateKeyboards;

public class TemplateCreateKbWithBoards : TemplateCreationBaseHandler
{
    private readonly BoardsKeyboard _boardsKeyboard;
    private readonly BotMessenger _botMessenger;

    public TemplateCreateKbWithBoards(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TemplatesDbOperations templateDbOperations, Verifier verifier, BoardsKeyboard boardsKeyboard,
        BotMessenger botMessenger) : base(botClient, userDbOperations, templateDbOperations, verifier)
    {
        _boardsKeyboard = boardsKeyboard;
        _botMessenger = botMessenger;
    }

    protected override async Task HandleTask(Template template)
    {
        InlineKeyboardMarkup inlineKeyboardMarkup = await _boardsKeyboard.KeyboardBoardChoice(template.UserId, isTemplate: true);
        
        await _botMessenger.SendMessage(text: "We will start with choosing a board for our templated task:",
            chatId: template.UserId,
            replyKeyboardMarkup: inlineKeyboardMarkup);
    }
}