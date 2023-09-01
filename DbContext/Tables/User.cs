using System.ComponentModel.DataAnnotations;

namespace TelegramToTrello;

public class User
{
    [Key]
    public int TelegramId { get; set; }
    public string? TelegramName { get; set; }
    public string? TrelloToken { get; set; }
    public string? TrelloId { get; set; }
    public string? TrelloName { get; set; }
    public bool NotificationsEnabled { get; set; }

    public ICollection<Board>? Boards { get; set; }
}