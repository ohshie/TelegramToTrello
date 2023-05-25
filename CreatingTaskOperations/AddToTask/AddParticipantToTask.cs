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
        CreatingTaskDbOperations dbOperations = new(user, task);
        if (participantName == "press this when done")
        {
            await FinishAddingParticipants(task);
            return;
        }
        
        bool userFoundOnBoard = await dbOperations.AddParticipant(participantName);
        if (!userFoundOnBoard)
        {
            await BotClient.SendTextMessageAsync(text: "Please choose name from keyboard menu.",
                chatId: Message.Chat.Id);
            return;
        }
        
        NextTask = new CreateKeyboardWithUsers(CallbackQuery, BotClient);
    }

    private async Task FinishAddingParticipants(TTTTask task)
    {
        await BotClient.DeleteMessageAsync(chatId: CallbackQuery.Message.Chat.Id, CallbackQuery.Message.MessageId);
        if (task.InEditMode)
        {
            TaskDbOperations taskDbOperations = new();
            await taskDbOperations.ToggleEditModeForTask(task);
            NextTask = new DisplayCurrentTaskInfo(CallbackQuery, BotClient);
        }
        else
        {
            NextTask = new TaskDateRequest(CallbackQuery, BotClient);
        }
    }
}