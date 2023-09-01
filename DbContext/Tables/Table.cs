using System.ComponentModel.DataAnnotations;

namespace TelegramToTrello;

public class Table
{
    [Key]
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? TableId { get; set; }
    public int BoardId { get; set; }
    
    public Board? TrelloUserBoard { get; set; }
}