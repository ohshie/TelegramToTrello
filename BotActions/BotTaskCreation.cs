using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.BotActions;

public class BotTaskCreation
{
    private TrelloOperations _trelloOperations = new TrelloOperations();
    
    private Message Message { get; }
    private TTTTask UserTask { get; set; }
    private RegisteredUser TrelloUser { get; set; }
    private CreatingTaskDbOperations CreatingTaskDbOperations { get; set; }
    private DbOperations DbOperations { get; set; }

    private ITelegramBotClient BotClient { get; }

    public BotTaskCreation(ITelegramBotClient botClient, Message message)
    {
        BotClient = botClient;
        Message = message;
        UserTask = null;
        TrelloUser = null;
    }

    private async Task GetTrelloUserAndTask()
    {
        DbOperations = new DbOperations();
        
        TrelloUser = await DbOperations.RetrieveTrelloUser((int)Message.From.Id);

        if (TrelloUser != null)
        {
            UserTask = await DbOperations.RetrieveUserTask((int)Message.From.Id);
        }
        
        CreatingTaskDbOperations = new CreatingTaskDbOperations(TrelloUser, UserTask);
    }

    private async Task SendNotRegisteredMessage()
    {
        BotClient.SendTextMessageAsync(text: "Looks like you are not registered yet." + 
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
        
        // step 1
        if (Message.Text.StartsWith("/board")) await NewTaskBoardSelection();
        
        // step 2
        if (Message.Text.StartsWith("/list")) await NewTaskTableSelection();
        
        // step 3
        if (Message.Text.StartsWith("/tag")) await NewTaskChanelTagSelection();
        
        // step 4 
        if (Message.Text.StartsWith("/desc")) await GetDescriptionFromUser();
        
        // step 5
        if (Message.Text.StartsWith("/part")) await ChoosingParticipants();
        if (Message.Text.StartsWith("/name")) await AddParticipantToTask();
        
        // step 6
        if (Message.Text.StartsWith("/date")) await AskUserForADate();
        
        // step 7
        if (Message.Text.StartsWith("/push")) await PushTaskToTrello();
    }
    
    // step 1 getting a board for new task
    private async Task NewTaskBoard()
    {
        // creating bot keyboard with user boards
        ReplyKeyboardMarkup replyKeyboardMarkup = KeyboardBoardChoice();
        await BotClient.SendTextMessageAsync(text: "We will start with choosing a board for our task:",
            chatId: Message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: Message.MessageId);
    }
    
    private ReplyKeyboardMarkup KeyboardBoardChoice()
    {
        List<KeyboardButton[]> keyboardButtonsList = new List<KeyboardButton[]>();
        
        foreach (var board in TrelloUser.UsersBoards.Select(ub => ub.Boards))
        {
            keyboardButtonsList.Add(new KeyboardButton[] { new KeyboardButton($"/board {board.BoardName}") });
        }

        ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardButtonsList)
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true,
            Selective = true
        };

        return replyKeyboardMarkup;
    }

    private async Task NewTaskBoardSelection()
    {
        string boardName = Message.Text.Substring("/board".Length).Trim();
        Console.WriteLine(boardName);

        bool boardExist = await CreatingTaskDbOperations.AddBoardToTask(boardName);
        if (!boardExist)
        {
            await BotClient.SendTextMessageAsync(text: "Please choose board name from keyboard menu.",
                chatId: Message.Chat.Id,
                replyToMessageId: Message.MessageId);
            return;
        }

        ReplyKeyboardMarkup replyKeyboardMarkup = await KeyboardTableChoice();

        await BotClient.SendTextMessageAsync(text: $"Now choose list on {boardName}",
            chatId: Message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: Message.MessageId);
    }

    // step 2 getting a table/list for a task on selected board
    private async Task<ReplyKeyboardMarkup> KeyboardTableChoice()
    {
        Board selectedBoard = await DbOperations.RetrieveBoards(UserTask.Id, UserTask.TrelloBoardId);

        List<KeyboardButton[]> keyboardButtonsList = new List<KeyboardButton[]>();

        foreach (var table in selectedBoard.Tables)
        {
            keyboardButtonsList.Add(new KeyboardButton[] { new KeyboardButton($"/list {table.Name}") });
        }

        ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardButtonsList)
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true,
            Selective = true
        };

        return replyKeyboardMarkup;
    }

    private async Task NewTaskTableSelection()
    {
        string listName = Message.Text.Substring("/list".Length).Trim();
        
        bool listExist = await CreatingTaskDbOperations.AddTableToTask(listName);
        if (!listExist)
        {
            await BotClient.SendTextMessageAsync(text: "Please choose list name from keyboard menu.",
                chatId: Message.Chat.Id,
                replyToMessageId: Message.MessageId);
            return;
        }

        ReplyKeyboardMarkup replyKeyboardMarkup = KeyboardTagChoice();

        await BotClient.SendTextMessageAsync(text: "Choose channel tag according to your task channel",
            chatId: Message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: Message.MessageId);
    }

    // step 3 getting chanel tag for new task.
    private ReplyKeyboardMarkup KeyboardTagChoice()
    {
        List<KeyboardButton[]> keyboardButtonsList = new List<KeyboardButton[]>();

        foreach (var tag in Enum.GetValues(typeof(ChanelTags)))
        {
            keyboardButtonsList.Add(new KeyboardButton[] { new KeyboardButton($"/tag {tag}") });
        }

        ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardButtonsList)
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true,
            Selective = true
        };

        return replyKeyboardMarkup;
    }

    private async Task NewTaskChanelTagSelection()
    {
        string tag = Message.Text.Substring("/tag".Length).Trim();
        
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
                
        await BotClient.SendTextMessageAsync(Message.Chat.Id,
            replyToMessageId: Message.MessageId,
            text: $"Task name successfully set to: {Message.Text}\n" +
                  $"Press /desc when ready to add description.");
    }
    
    // step 5 adding description to the task   
    private async Task GetDescriptionFromUser()
    {
        if (!UserTask.NameSet) return;

        await CreatingTaskDbOperations.AddPlaceholderDescription();
        
        await BotClient.SendTextMessageAsync(text: "Now please type description of your task in the next message.",
            chatId: Message.Chat.Id,
            replyMarkup: new ReplyKeyboardRemove(),
            replyToMessageId: Message.MessageId);
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
        ReplyKeyboardMarkup replyKeyboardMarkup = await KeyboardParticipants();
        
        await BotClient.SendTextMessageAsync(text: "choose participant from a list",
            chatId: Message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: Message.MessageId);
    }

    private async Task<ReplyKeyboardMarkup> KeyboardParticipants()
    {
        List<KeyboardButton[]> keyboardButtonsList = new List<KeyboardButton[]>();

        Board taskBoards = await DbOperations.RetrieveBoards(UserTask.Id, UserTask.TrelloBoardId);

        if (taskBoards != null)
        {
            keyboardButtonsList.Add(new KeyboardButton[] {new KeyboardButton("/name press this when done")});

            IEnumerable<UsersOnBoard> filteredUsers = taskBoards.UsersOnBoards;
            
            if (UserTask.TaskPartName.Length > 0)
            {
                string addedUsers = UserTask.TaskPartName.Remove(UserTask.TaskPartName.Length-1);
                List<string> addedUsersList = addedUsers.Split(',').ToList();
                filteredUsers = taskBoards.UsersOnBoards.Where(uob => !addedUsersList.Contains(uob.Name));
            }

            foreach (var user in filteredUsers)
            {
                keyboardButtonsList.Add(new KeyboardButton[] {new KeyboardButton($"/name {user.Name}")});
            }
        }

        ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(keyboardButtonsList)
        {
            OneTimeKeyboard = true,
            ResizeKeyboard = true,
            Selective = true
        };
        
        return replyKeyboardMarkup;
    }

    private async Task AddParticipantToTask()
    {
        string participantName = Message.Text.Substring("/part".Length).Trim();
        
        if (participantName == "press this when done")
        {
            await BotClient.SendTextMessageAsync(
                chatId: Message.Chat.Id,
                text: "All participants added\n" +
                      "All is left is to add a due date\n" +
                      "Press /date when ready",
                      replyMarkup: new ReplyKeyboardRemove(),
                replyToMessageId: Message.MessageId);
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
        
        ReplyKeyboardMarkup replyKeyboardMarkup = await KeyboardParticipants();

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
    
        await BotClient.SendTextMessageAsync(text: "Please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
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
                text: "You are pushing tag to early, please follow bot commands.",
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