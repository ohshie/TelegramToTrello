using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.BotActions;

public class BotTaskCreation
{
    private TrelloOperations _trelloOperations = new TrelloOperations();
    
    private Message Message { get; }
    private TTTTask? UserTask { get; set; } = null;
    private RegisteredUser? TrelloUser { get; set; } = null;
    private CreatingTaskDbOperations CreatingTaskDbOperations { get; set; }
    private DbOperations DbOperations { get; set; }
    public  CallbackQuery CallbackQuery { get; set; }

    private ITelegramBotClient BotClient { get; }

    public BotTaskCreation(ITelegramBotClient botClient, Message message)
    {
        BotClient = botClient;
        Message = message;
    }

    public BotTaskCreation(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        BotClient = botClient;
        CallbackQuery = callbackQuery;
        Message = callbackQuery.Message;
    }

    private async Task GetTrelloUserAndTask()
    {
        DbOperations = new DbOperations();
        
        TrelloUser = await DbOperations.RetrieveTrelloUser((int)Message.Chat.Id);
        if (TrelloUser != null)
        {   
            UserTask = await DbOperations.RetrieveUserTask((int)Message.Chat.Id);
        }
        
        CreatingTaskDbOperations = new CreatingTaskDbOperations(TrelloUser, UserTask);
    }

    private async Task SendNotRegisteredMessage()
    {
        await BotClient.SendTextMessageAsync(text: "Looks like you are not registered yet." + 
                                             "Click on /register and follow commands to register",
            chatId: Message.Chat.Id,
            replyToMessageId: Message.MessageId);
    }
    
    public async Task InitialTaskCreator()
    {
        
        await GetTrelloUserAndTask();
        if (TrelloUser == null)
        {
            await SendNotRegisteredMessage();
            return;
        }
        
        if (TrelloUser != null)
        {
            await CreatingTaskDbOperations.AddTaskToDb();
            await NewTaskBoard();
            return;
        }
        
        await BotClient.SendTextMessageAsync(text: "Looks like task is already in progress.\n" +
                                                   "Please finish task creation by following bot commands",
            chatId: Message.Chat.Id,
            replyToMessageId: Message.MessageId);
    }

    public async Task TaskCreationOperator()
    {
        await GetTrelloUserAndTask();
        if (TrelloUser == null)
        {
            await SendNotRegisteredMessage();
            return;
        }
        
        if (UserTask == null)
        {
            await BotClient.SendTextMessageAsync(text: "Lets not get ahead of ourselves." +
                                                       "Click on /newtask first to start task creation process",
                chatId: Message.Chat.Id,
                replyToMessageId: Message.MessageId);
            return;
        }

        // step 5
        if (Message.Text.StartsWith("/part")) await ChoosingParticipants();

        // step 6
        if (Message.Text.StartsWith("/date")) await AskUserForADate();
        
        // step 7
        if (Message.Text.StartsWith("/push")) await PushTaskToTrello();
    }

    public async Task HandleInlineKeyBoardCallBack()
    {
        await GetTrelloUserAndTask();
        if (TrelloUser == null)
        {
            await SendNotRegisteredMessage();
            return;
        }
        
        if (UserTask == null)
        {
            await BotClient.SendTextMessageAsync(text: "Lets not get ahead of ourselves." +
                                                       "Click on /newtask first to start task creation process",
                chatId: Message.Chat.Id,
                replyToMessageId: Message.MessageId);
            return;
        }
        
        // step 1
        if (CallbackQuery.Data.StartsWith("/board")) await NewTaskBoardSelection();
        // step 2
        if (CallbackQuery.Data.StartsWith("/list")) await NewTaskTableSelection();
        // step 3
        if (CallbackQuery.Data.StartsWith("/tag")) await NewTaskChanelTagSelection();
        
        if (CallbackQuery.Data.StartsWith("/name")) await AddParticipantToTask();
        
    }
    
    // step 1 getting a board for new task
    private async Task NewTaskBoard()
    {
        // creating bot keyboard with user boards
        InlineKeyboardMarkup inlineKeyboardMarkup = KeyboardBoardChoice();
        await BotClient.SendTextMessageAsync(text: "We will start with choosing a board for our task:",
            chatId: Message.Chat.Id,
            replyMarkup: inlineKeyboardMarkup,
            replyToMessageId: Message.MessageId);
    }
    
    private InlineKeyboardMarkup KeyboardBoardChoice()
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();
        
        foreach (var board in TrelloUser.UsersBoards.Select(ub => ub.Boards))
        {
            keyboardButtonsList.Add(new InlineKeyboardButton[]
                {  InlineKeyboardButton.WithCallbackData($"{board.BoardName}",$"/board {board.TrelloBoardId}") });
        }

        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(keyboardButtonsList);

        return inlineKeyboard;
    }
    
    private async Task NewTaskBoardSelection()
    {
        string boardId = CallbackQuery.Data.Substring("/board".Length).Trim();
        Console.WriteLine(boardId);

        string? boardName = await CreatingTaskDbOperations.AddBoardToTask(boardId);
        if (string.IsNullOrEmpty(boardName))
        {
            await BotClient.SendTextMessageAsync(text: "Please choose board name from keyboard menu.",
                chatId: Message.Chat.Id,
                replyToMessageId: Message.MessageId);
            return;
        }

        InlineKeyboardMarkup replyKeyboardMarkup = await KeyboardTableChoice();

        await BotClient.SendTextMessageAsync(text: $"Now choose list on {boardName}",
            chatId: Message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: Message.MessageId);
    }

    // step 2 getting a table/list for a task on selected board
    private async Task<InlineKeyboardMarkup> KeyboardTableChoice()
    {
        Board selectedBoard = await DbOperations.RetrieveBoard(UserTask.Id, UserTask.TrelloBoardId);

        List<InlineKeyboardButton[]> keyboardButtonsList = new List<InlineKeyboardButton[]>();

        foreach (var table in selectedBoard.Tables)
        {
            keyboardButtonsList.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData($"{table.Name}",$"/list {table.Name}")
            });
        }

        InlineKeyboardMarkup replyKeyboardMarkup = new InlineKeyboardMarkup(keyboardButtonsList);

        return replyKeyboardMarkup;
    }

    private async Task NewTaskTableSelection()
    {
        string listName = CallbackQuery.Data.Substring("/list".Length).Trim();
        
        bool listExist = await CreatingTaskDbOperations.AddTableToTask(listName);
        if (!listExist)
        {
            await BotClient.SendTextMessageAsync(text: "Please choose list name from keyboard menu.",
                chatId: Message.Chat.Id,
                replyToMessageId: Message.MessageId);
            return;
        }

        InlineKeyboardMarkup replyKeyboardMarkup = KeyboardTagChoice();

        await BotClient.SendTextMessageAsync(text: "Choose channel tag according to your task channel",
            chatId: Message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: Message.MessageId);
    }

    // step 3 getting chanel tag for new task.
    private InlineKeyboardMarkup KeyboardTagChoice()
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();

        foreach (var tag in Enum.GetValues(typeof(ChanelTags)))
        {
            keyboardButtonsList.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData($"{tag}",$"/tag {tag}") });
        }

        InlineKeyboardMarkup replyKeyboardMarkup = new(keyboardButtonsList);

        return replyKeyboardMarkup;
    }

    private async Task NewTaskChanelTagSelection()
    {
        string tag =  CallbackQuery.Data.Substring("/tag".Length).Trim();
        
        if (!(Enum.TryParse(typeof(ChanelTags), tag, true, out _)))
        {
            await BotClient.SendTextMessageAsync(text: "Please choose tag from keyboard menu.",
                chatId: Message.Chat.Id,
                replyToMessageId: Message.MessageId);
            return;
        }
        
        Task addTag = CreatingTaskDbOperations.AddTagToTask(tag);
        Task addPlaceholder = CreatingTaskDbOperations.AddPlaceholderName();

        await Task.WhenAll(addTag, addPlaceholder);
        
        await BotClient.SendTextMessageAsync(text: "Now please type name of your task in the next message.",
            chatId: Message.Chat.Id,
            replyMarkup: new ReplyKeyboardRemove(),
            replyToMessageId: Message.MessageId);
    }
    
    // strep 4 adding name to task
    private async Task AddNameToTask()
    {
        if (Message.Text.StartsWith("/"))
        {
            await BotClient.SendTextMessageAsync(Message.Chat.Id,
                replyToMessageId: Message.MessageId,
                text: $"Task name should not start with \"/\"\n" +
                      $"Please type a new name for a task");
            return;
        }
        
        await CreatingTaskDbOperations.SetTaskName(Message.Text);
        await CreatingTaskDbOperations.AddPlaceholderDescription();
        
        await BotClient.SendTextMessageAsync(Message.Chat.Id,
            replyToMessageId: Message.MessageId,
            text: $"Task name successfully set to: {Message.Text}\n" +
                  $"Now please type description of your task in the next message.");
        
        
    }
    
    private async Task AddDescriptionToTask()
    {
        if (Message.Text.StartsWith("/"))
        {
            await BotClient.SendTextMessageAsync(Message.Chat.Id,
                replyToMessageId: Message.MessageId,
                text: $"Task description should not start with \"/\"\n" +
                      $"Please type a new description for a task");
            return;
        }
        
        await CreatingTaskDbOperations.SetDescription(Message.Text);

        await BotClient.SendTextMessageAsync(Message.Chat.Id,
            replyToMessageId: Message.MessageId,
            text: $"Task description successfully added\n" +
                  $"Press /part when ready to add participants");
    }
    
    // step 6 adding participants
    private async Task ChoosingParticipants()
    {
        InlineKeyboardMarkup replyKeyboardMarkup = await KeyboardParticipants();
        
        await BotClient.SendTextMessageAsync(text: "choose participant from a list",
            chatId: Message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: Message.MessageId);
    }

    private async Task<InlineKeyboardMarkup> KeyboardParticipants()
    {
        List<InlineKeyboardButton[]> keyboardButtonsList = new();

        Board taskBoard = await DbOperations.RetrieveBoard(UserTask.Id, UserTask.TrelloBoardId);

        if (taskBoard != null)
        {
            IEnumerable<UsersOnBoard> filteredUsers = taskBoard.UsersOnBoards;
            
            if (UserTask.TaskPartName.Length > 0)
            {
                string addedUsers = UserTask.TaskPartName.Remove(UserTask.TaskPartName.Length-1);
                List<string> addedUsersList = addedUsers.Split(',').ToList();
                filteredUsers = taskBoard.UsersOnBoards.Where(uob => !addedUsersList.Contains(uob.Name));
            }

            foreach (var user in filteredUsers)
            {
                keyboardButtonsList.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData($"{user.Name}",$"/name {user.Name}")});
            }
            
            keyboardButtonsList.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("press this when done","/name press this when done") });
        }

        InlineKeyboardMarkup replyKeyboardMarkup = new(keyboardButtonsList);

        return replyKeyboardMarkup;
    }

    private async Task AddParticipantToTask()
    {
        string participantName = CallbackQuery.Data.Substring("/name".Length).Trim();
        
        if (participantName == "press this when done")
        {
            await AskUserForADate();
            return;
        }
        
        bool userFoundOnBoard = await CreatingTaskDbOperations.AddParticipantToTask(participantName);
        if (!userFoundOnBoard)
        {
            await BotClient.SendTextMessageAsync(text: "Please choose name from keyboard menu.",
                chatId: Message.Chat.Id,
                replyToMessageId: Message.MessageId);
            return;
        }
        
        InlineKeyboardMarkup replyKeyboardMarkup = await KeyboardParticipants();

        await BotClient.SendTextMessageAsync(text: $"{participantName} added to task: {UserTask.TaskName}",
            chatId: Message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: Message.MessageId);
    }
    
    // step 7 add date
    private async Task AskUserForADate()
    {
        if (!UserTask.DescSet) return;
        
        await CreatingTaskDbOperations.AddPlaceholderDate();
        
        await BotClient.SendTextMessageAsync(text: "All participants added\n\n" +
                                                   "Now please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                                                       "Due date must be in the future.",
            chatId: Message.Chat.Id,
            replyToMessageId: Message.MessageId);
    }

    private async Task AddDateToTask()
    {
        if (Message.Text.StartsWith("/"))
        {
            await BotClient.SendTextMessageAsync(Message.Chat.Id,
                replyToMessageId: Message.MessageId,
                text: $"Task date should not start with \"/\"\n" +
                      $"Please type a new date for a task.");
            return;
        }
        
        string possibleDate = dateConverter(Message.Text);
        if (possibleDate == null)
        {
            await BotClient.SendTextMessageAsync(text: "Please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                                                       "Due date must be in the future.",
                chatId: Message.Chat.Id,
                replyToMessageId: Message.MessageId);
            return;
        }
        
        await CreatingTaskDbOperations.AddDateToTask(possibleDate);

        await DisplayCurrentTaskInfo();
    }

    private async Task DisplayCurrentTaskInfo()
    {
        string boardName = await DbOperations.BoardIdToName(UserTask.TrelloBoardId);
        
        await BotClient.SendTextMessageAsync(text: "Lets review current task:\n\n" +
                                                   $"Task name: [{UserTask.Tag}] {UserTask.TaskName}\n" +
                                                   $"On board: {boardName}\n"+
                                                   $"Description: {UserTask.TaskDesc}\n"+
                                                   $"Participants: {UserTask.TaskPartName}\n"+
                                                   $"Due date: {DateTime.Parse(UserTask.Date)}\n\n" +
                                                   $"If everything is correct press /push to post this task to trello\n",
            chatId: Message.Chat.Id,
            replyToMessageId: Message.MessageId);
    }
    
    private string dateConverter(string date)
    {
        DateTime properDate;
        DateTime.TryParseExact(date, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None,
            out properDate);
        if (properDate < DateTime.Today) return null;
       
        if (DateTime.TryParseExact(date, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out properDate))
        {
            string trelloDate = properDate.ToString("o");
            return trelloDate;
        }
        
        return null;
    }
    
    private async Task PushTaskToTrello()
    {
        if (string.IsNullOrEmpty(UserTask.TrelloBoardId) | string.IsNullOrEmpty(UserTask.ListId) |
            string.IsNullOrEmpty(UserTask.Tag))
        {
            await BotClient.SendTextMessageAsync(Message.Chat.Id,
                text: "You are pushing task to early, please follow bot commands.",
                replyToMessageId: Message.MessageId);
            return;
        }
        
        bool success = await _trelloOperations.PushTaskToTrello(UserTask);
        if (success)
        {
            await BotClient.SendTextMessageAsync(Message.Chat.Id,
                text: "Task successfully created",
                replyToMessageId: Message.MessageId);
            await RemoveTaskFromDb();
        }
    }
    
    public async Task MessagesToReplacePlaceholdersWithValues()
    {
        await GetTrelloUserAndTask();
        
        if (UserTask == null) return;
            
            if (UserTask.TaskName == "###tempname###")
            {
                await AddNameToTask();
                return;
            }
            if (UserTask.TaskDesc == "###tempdesc###")
            {
                await AddDescriptionToTask();
                return;
            }
            if (UserTask.Date == "###tempdate###")
            {
                await AddDateToTask();
            }
    }
    
    private async Task RemoveTaskFromDb() => DbOperations.RemoveEntry(UserTask);
    
}