using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramToTrello.BotActions;

public class BotTaskCreation
{
    private DbOperations _dbOperation = new DbOperations();
    private CreatingTaskDbOperations _creatingTaskDbOperations = new CreatingTaskDbOperations();
    private TrelloOperations _trelloOperations = new TrelloOperations();

    private ITelegramBotClient BotClient { get; }

    public BotTaskCreation(ITelegramBotClient botClient)
    {
        BotClient = botClient;
    }

    public async Task InitialTaskCreator(Message message)
    {
        int telegramId = (int)message.From.Id;
        RegisteredUsers? trelloUser = await _dbOperation.RetrieveTrelloUser(telegramId);
        if (trelloUser == null)
        {
            await BotClient.SendTextMessageAsync(text: "Looks like you are not registered yet." +
                                                       "Click on /register and follow commands to register",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }
        
        TTTTask userTask = await _dbOperation.RetrieveUserTask(trelloUser.TelegramId);
        if (userTask == null)
        {
            await _creatingTaskDbOperations.AddTaskToDb(trelloUser);
            
            await NewTaskBoard(message, trelloUser);
            return;
        }
        
        await BotClient.SendTextMessageAsync(text: "Looks like task is already in progress.\n" +
                                                   "Please finish task creation by following bot commands",
            chatId: message.Chat.Id,
            replyToMessageId: message.MessageId);
    }

    public async Task TaskCreationOperator(Message message)
    {
       
        int telegramId = (int)message.From.Id;
        RegisteredUsers? trelloUser = await _dbOperation.RetrieveTrelloUser(telegramId);
        if (trelloUser == null)
        {
            await BotClient.SendTextMessageAsync(text: "Looks like you are not registered yet." +
                                                       "Click on /register and follow commands to register",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }
        
        TTTTask userTask = await _dbOperation.RetrieveUserTask(trelloUser.TelegramId);
        if (userTask == null)
        {
            await BotClient.SendTextMessageAsync(text: "Lets not get ahead of ourselves." +
                                                       "Click on /newtask first to start task creation process",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }
        
        // step 1
        if (message.Text.StartsWith("/board")) await NewTaskBoardSelection(message, userTask);
        
        // step 2
        if (message.Text.StartsWith("/list")) await NewTaskTableSelection(message, userTask);
        
        // step 3
        if (message.Text.StartsWith("/tag")) await NewTaskChanelTagSelection(userTask, message);
        
        // step 4 
        if (message.Text.StartsWith("/desc")) await GetDescriptionFromUser(message, userTask);
        
        // step 5
        if (message.Text.StartsWith("/part")) await ChoosingParticipants(message, userTask);
        if (message.Text.StartsWith("/name")) await AddParticipantToTask(message, userTask);
        
        // step 6
        if (message.Text.StartsWith("/date")) await AskUserForADate(message, userTask);
        
        // step 7
        if (message.Text.StartsWith("/push")) await PushTaskToTrello(message, userTask);
    }
    
    // step 1 getting a board for new task
    private async Task NewTaskBoard(Message message, RegisteredUsers trelloUser)
    {
        // creating bot keyboard with user boards
        ReplyKeyboardMarkup replyKeyboardMarkup = KeyboardBoardChoice(trelloUser);
        await BotClient.SendTextMessageAsync(text: "We will start with choosing a board for our task:",
            chatId: message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: message.MessageId);
    }
    
    private ReplyKeyboardMarkup KeyboardBoardChoice(RegisteredUsers trelloUser)
    {
        List<KeyboardButton[]> keyboardButtonsList = new List<KeyboardButton[]>();
        
        foreach (var board in trelloUser.UsersBoards.Select(ub => ub.Boards))
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

    private async Task NewTaskBoardSelection(Message message, TTTTask userTask)
    {
        string boardName = message.Text.Substring("/board".Length).Trim();
        Console.WriteLine(boardName);

        bool boardExist = await _creatingTaskDbOperations.AddBoardToTask(userTask, boardName);
        if (!boardExist)
        {
            await BotClient.SendTextMessageAsync(text: "Please choose board name from keyboard menu.",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }

        ReplyKeyboardMarkup replyKeyboardMarkup = await KeyboardTableChoice(userTask);

        await BotClient.SendTextMessageAsync(text: $"Now choose list on {boardName}",
            chatId: message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: message.MessageId);
    }

    // step 2 getting a table/list for a task on selected board
    private async Task<ReplyKeyboardMarkup> KeyboardTableChoice(TTTTask userTask)
    {
        Boards selectedBoard = await _dbOperation.RetrieveBoards(userTask.Id, userTask.TrelloBoardId);

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

    private async Task NewTaskTableSelection(Message message, TTTTask userTask)
    {
        string listName = message.Text.Substring("/list".Length).Trim();
        
        bool listExist = await _creatingTaskDbOperations.AddTableToTask(userTask, listName);
        if (!listExist)
        {
            await BotClient.SendTextMessageAsync(text: "Please choose list name from keyboard menu.",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }

        ReplyKeyboardMarkup replyKeyboardMarkup = KeyboardTagChoice();

        await BotClient.SendTextMessageAsync(text: "Choose channel tag according to your task channel",
            chatId: message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: message.MessageId);
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

    private async Task NewTaskChanelTagSelection(TTTTask userTask, Message message)
    {
        string tag = message.Text.Substring("/tag".Length).Trim();
        
        if (!(Enum.TryParse(typeof(ChanelTags), tag, true, out _)))
        {
            await BotClient.SendTextMessageAsync(text: "Please choose tag from keyboard menu.",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }
        
        _creatingTaskDbOperations.AddTagToTask(userTask, tag);

        _creatingTaskDbOperations.AddPlaceholderName(userTask);
        
        await BotClient.SendTextMessageAsync(text: "Now please type name of your task in the next message.",
            chatId: message.Chat.Id,
            replyMarkup: new ReplyKeyboardRemove(),
            replyToMessageId: message.MessageId);
    }
    
    // strep 4 adding name to task
    public async Task AddNameToTask(TTTTask task, Message message)
    {
        if (message.Text.StartsWith("/"))
        {
            await BotClient.SendTextMessageAsync(message.Chat.Id,
                replyToMessageId: message.MessageId,
                text: $"Task name should not start with \"/\"\n" +
                      $"Please type a new name for a task");
            return;
        }
        
        await _creatingTaskDbOperations.SetTaskName(task, message.Text);
                
        await BotClient.SendTextMessageAsync(message.Chat.Id,
            replyToMessageId: message.MessageId,
            text: $"Task name succesfully set to: {message.Text}\n" +
                  $"Press /desc when ready to add description.");
    }
    
    // step 5 adding description to the task   
    private async Task GetDescriptionFromUser(Message message, TTTTask userTask)
    {
        if (!userTask.NameSet) return;

        await _creatingTaskDbOperations.AddPlaceholderDescription(userTask);
        
        await BotClient.SendTextMessageAsync(text: "Now please type description of your task in the next message.",
            chatId: message.Chat.Id,
            replyMarkup: new ReplyKeyboardRemove(),
            replyToMessageId: message.MessageId);
    }

    public async Task AddDescriptionToTask(TTTTask task, Message message)
    {
        if (message.Text.StartsWith("/"))
        {
            await BotClient.SendTextMessageAsync(message.Chat.Id,
                replyToMessageId: message.MessageId,
                text: $"Task description should not start with \"/\"\n" +
                      $"Please type a new description for a task");
            return;
        }
        
        await _creatingTaskDbOperations.SetDescription(task, message.Text);

        await BotClient.SendTextMessageAsync(message.Chat.Id,
            replyToMessageId: message.MessageId,
            text: $"Task description successfully added\n" +
                  $"Press /part when ready to add participants");
    }
    
    // step 6 adding participants
    private async Task ChoosingParticipants(Message message, TTTTask userTask)
    {
        ReplyKeyboardMarkup replyKeyboardMarkup = await KeyboardParticipants(userTask);
        
        await BotClient.SendTextMessageAsync(text: "choose participant from a list",
            chatId: message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: message.MessageId);
    }

    private async Task<ReplyKeyboardMarkup> KeyboardParticipants(TTTTask task)
    {
        List<KeyboardButton[]> keyboardButtonsList = new List<KeyboardButton[]>();

        Boards taskBoards = await _dbOperation.RetrieveBoards(task.Id, task.TrelloBoardId);

        if (taskBoards != null)
        {
            keyboardButtonsList.Add(new KeyboardButton[] {new KeyboardButton("/name press this when done")});

            IEnumerable<UsersOnBoard> filteredUsers = taskBoards.UsersOnBoards;
            
            if (task.TaskPartName.Length > 0)
            {
                string addedUsers = task.TaskPartName.Remove(task.TaskPartName.Length-1);
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

    private async Task AddParticipantToTask(Message message, TTTTask userTask)
    {
        string participantName = message.Text.Substring("/part".Length).Trim();
        
        if (participantName == "press this when done")
        {
            await BotClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "All participants added\n" +
                      "All is left is to add a due date\n" +
                      "Press /date when ready",
                      replyMarkup: new ReplyKeyboardRemove(),
                replyToMessageId: message.MessageId);
            return;
        }
        
        bool userFoundOnBoard = await _creatingTaskDbOperations.AddParticipantToTask(userTask, participantName);
        if (!userFoundOnBoard)
        {
            await BotClient.SendTextMessageAsync(text: "Please choose name from keyboard menu.",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }
        
        ReplyKeyboardMarkup replyKeyboardMarkup = await KeyboardParticipants(userTask);

        await BotClient.SendTextMessageAsync(text: $"{participantName} added to task: {userTask.TaskName}",
            chatId: message.Chat.Id,
            replyMarkup: replyKeyboardMarkup,
            replyToMessageId: message.MessageId);
    }
    
    // step 7 add date
    private async Task AskUserForADate(Message message, TTTTask userTask)
    {
        if (!userTask.DescSet) return;
        
        await _creatingTaskDbOperations.AddPlaceholderDate(userTask);
    
        await BotClient.SendTextMessageAsync(text: "Please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                                                       "Due date must be in the future.",
            chatId: message.Chat.Id,
            replyToMessageId: message.MessageId);
    }

    public async Task AddDateToTask(TTTTask task, Message message)
    {
        if (message.Text.StartsWith("/"))
        {
            await BotClient.SendTextMessageAsync(message.Chat.Id,
                replyToMessageId: message.MessageId,
                text: $"Task date should not start with \"/\"\n" +
                      $"Please type a new date for a task.");
            return;
        }
        
        string possibleDate = dateConverter(message.Text);
        if (possibleDate == null)
        {
            await BotClient.SendTextMessageAsync(text: "Please enter date in the format like this - 24.02.2022 04:30 (dd.mm.yyyy hh:mm)\n" +
                                                       "Due date must be in the future.",
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId);
            return;
        }
        
        await _creatingTaskDbOperations.AddDateToTask(task, possibleDate);

        await DisplayCurrentTaskInfo(message, task);
    }

    private async Task DisplayCurrentTaskInfo(Message message, TTTTask task)
    {
        string boardName = await _dbOperation.BoardIdToName(task.TrelloBoardId);
        
        await BotClient.SendTextMessageAsync(text: "Lets review current task:\n\n" +
                                                   $"Task name: [{task.Tag}] {task.TaskName}\n" +
                                                   $"On board: {boardName}\n"+
                                                   $"Description: {task.TaskDesc}\n"+
                                                   $"Participants: {task.TaskPartName}\n"+
                                                   $"Due date: {DateTime.Parse(task.Date)}\n\n" +
                                                   $"If everything is correct press /push to post this task to trello\n",
            chatId: message.Chat.Id,
            replyToMessageId: message.MessageId);
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
    
    private async Task PushTaskToTrello(Message message, TTTTask userTask)
    {
        if (string.IsNullOrEmpty(userTask.TrelloBoardId) | string.IsNullOrEmpty(userTask.ListId) |
            string.IsNullOrEmpty(userTask.Tag))
        {
            await BotClient.SendTextMessageAsync(message.Chat.Id,
                text: "You are pushing tag to early, please follow bot commands.",
                replyToMessageId: message.MessageId);
            return;
        }
        
        bool success = await _trelloOperations.PushTaskToTrello(userTask);
        if (success)
        {
            await BotClient.SendTextMessageAsync(message.Chat.Id,
                text: "Task successfully created",
                replyToMessageId: message.MessageId);
            await RemoveTaskFromDb(userTask);
            return;
        }
    }

    private async Task RemoveTaskFromDb(TTTTask userTask)
    {
        _dbOperation.RemoveEntry(userTask);
    }
}