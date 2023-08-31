using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramToTrello.BotManager;

namespace TelegramToTrello.TemplateManager.CreatingTemplatesOperations.CreateKeyboards;

public class TemplateCreateKbWithBoards : TemplateCreationBaseHandler
{
    private readonly BoardsKeyboard _boardsKeyboard;

    public TemplateCreateKbWithBoards(ITelegramBotClient botClient, UserDbOperations userDbOperations,
        TemplatesDbOperations templateDbOperations, Verifier verifier, BoardsKeyboard boardsKeyboard) : base(botClient, userDbOperations, templateDbOperations, verifier)
    {
        _boardsKeyboard = boardsKeyboard;
    }

    protected override async Task HandleTask(RegisteredUser user, Template template)
    {
        InlineKeyboardMarkup inlineKeyboardMarkup = _boardsKeyboard.KeyboardBoardChoice(user, isTemplate: true);
        
        await BotClient.SendTextMessageAsync(text: "We will start with choosing a board for our templated task:",
            chatId: user.TelegramId,
            replyMarkup: inlineKeyboardMarkup);
    }
}