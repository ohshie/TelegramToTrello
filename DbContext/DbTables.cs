namespace TelegramToTrello;

public class TrelloUser
{
    public int Id { get; set; }
    public string TelegramUserName { get; set; }
    public string TrelloUserName { get; set; }
    public string TrelloId { get; set; }
    
    public ICollection<TrelloUserBoard> TrelloUserBoards { get; set; }
}

public class TrelloUserBoard
{
    public int Id { get; set; }
    public string TrelloBoardId { get; set; }
    public string Name { get; set; }
    public string TrelloUserId { get; set; }
    public int TelegramId { get; set; }
    public TrelloUser TrelloUser { get; set; }
    
    public ICollection<TrelloBoardTable> TrelloBoardTables { get; set; }
    public ICollection<UsersOnBoard> UsersOnBoards { get; set; }
}

public class TrelloBoardTable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string TableId { get; set; }
    public int BoardId { get; set; }
    
    public TrelloUserBoard TrelloUserBoard { get; set; }
}

public class UsersOnBoard
{
    public int Id { get; set; }
    public string TrelloUserId { get; set; }
    public string Name { get; set; }
    public int TrelloUserBoardId { get; set; }
    
    public TrelloUserBoard TrelloBoard { get; set; } 
}

public class TTTTask
{
    public int Id { get; set; }
    public string TaskName { get; set; }
    public string BoardId { get; set; }
    public string ListId { get; set; }
    public string TaskId { get; set; }
    public string TaskDesc { get; set; }
    public string TaskCurrentParticipant { get; set; }
}

