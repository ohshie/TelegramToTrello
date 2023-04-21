namespace TelegramToTrello;

public class RegisteredUsers
{
    public int TelegramId { get; set; }
    public string TelegramName { get; set; }
    public string TrelloToken { get; set; }
    public string TrelloId { get; set; }

    public ICollection<UsersBoards> UsersBoards { get; set; }
}

public class Boards
{
    public int Id { get; set; }
    public string TrelloBoardId { get; set; }
    public string BoardName { get; set; }
    public int TelegramId { get; set; }

    public ICollection<Tables> Tables { get; set; }
    public ICollection<UsersOnBoard> UsersOnBoards { get; set; }
    public ICollection<UsersBoards> UsersBoards { get; set; }
}

public class UsersBoards
{
    public int UserId { get; set; }
    public RegisteredUsers RegisteredUsers { get; set; }

    public int BoardId { get; set; }
    public Boards Boards { get; set; }
}

public class Tables
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string TableId { get; set; }
    public int BoardId { get; set; }
    
    public Boards TrelloUserBoard { get; set; }
}

public class UsersOnBoard
{
    public int Id { get; set; }
    public string TrelloUserId { get; set; }
    public string Name { get; set; }
    public int TrelloUserBoardId { get; set; }
    
    public Boards TrelloBoard { get; set; } 
}

public class TTTTask
{
    public int Id { get; set; }
    public string TaskName { get; set; }
    public string Tag { get; set; }
    public string BoardId { get; set; }
    public string ListId { get; set; }
    public string TaskId { get; set; }
    public string TaskDesc { get; set; }
    public string TaskCurrentParticipant { get; set; }
    public string Date { get; set; }
}

