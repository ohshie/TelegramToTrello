using System.ComponentModel.DataAnnotations;

namespace TelegramToTrello;

public class UsersOnBoard
{
    [Key]
    public int Id { get; set; }
    public string? TrelloUserId { get; set; }
    public string? Name { get; set; }
    public int TrelloUserBoardId { get; set; }
    
    public Board? TrelloBoard { get; set; }
}