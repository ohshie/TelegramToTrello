using System.ComponentModel.DataAnnotations;

namespace TelegramToTrello;

public class Board
{
    [Key]
    public int Id { get; set; }
    public string? TrelloBoardId { get; set; }
    public string? BoardName { get; set; }

    public ICollection<Table>? Tables { get; set; }
    public ICollection<UsersOnBoard>? UsersOnBoards { get; set; }
    public ICollection<User>? Users { get; set; } = new List<User>();
}