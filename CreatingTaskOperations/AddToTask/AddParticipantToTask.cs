using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramToTrello.CreatingTaskOperations;

public class AddParticipantToTask : TaskCreationBaseHandler
{
    public AddParticipantToTask(CallbackQuery callback, ITelegramBotClient botClient) : base(callback, botClient)
    {}

    protected override async Task HandleTask(RegisteredUser user, TTTTask task)
    {
        string participantName = CallbackQuery.Data.Substring("/name".Length).Trim();
        
        if (participantName == "press this when done")
        {
            await BotClient.DeleteMessageAsync(chatId: CallbackQuery.Message.Chat.Id, CallbackQuery.Message.MessageId);
            NextTask = new TaskDateRequest(CallbackQuery, BotClient);
            return;
        }

        CreatingTaskDbOperations dbOperations = new(user, task);
        bool userFoundOnBoard = await dbOperations.AddParticipantToTask(participantName);
        if (!userFoundOnBoard)
        {
            await BotClient.SendTextMessageAsync(text: "Please choose name from keyboard menu.",
                chatId: Message.Chat.Id);
            return;
        }
        
        NextTask = new CreateKeyboardWithUsers(CallbackQuery, BotClient);
    }
}