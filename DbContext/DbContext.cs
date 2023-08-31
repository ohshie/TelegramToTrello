using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello;

public class BotDbContext : DbContext
{
    public BotDbContext(DbContextOptions<BotDbContext> options) : base(options) { }
    
    public DbSet<RegisteredUser> Users { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<TTTTask> CreatingTasks { get; set; }
    public DbSet<Table> BoardTables { get; set; }
    public DbSet<UsersOnBoard> UsersOnBoards { get; set; }
    public DbSet<TaskNotification> TaskNotifications { get; set; }
    public DbSet<Template> Templates { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RegisteredUser>()
            .HasKey(u => u.TelegramId);

        modelBuilder.Entity<Template>()
            .HasKey(t => t.Id);

        modelBuilder.Entity<Board>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<Table>()
            .HasKey(bl => bl.Id);
        
        modelBuilder.Entity<UsersOnBoard>()
            .HasKey(uob => uob.Id);

        modelBuilder.Entity<RegisteredUser>()
            .HasMany(ru => ru.Boards)
            .WithMany(b => b.Users)
            .UsingEntity(etb => etb.ToTable("UsersBoards"));

        modelBuilder.Entity<UsersOnBoard>()
            .HasOne(uob => uob.TrelloBoard)
            .WithMany(tub => tub.UsersOnBoards)
            .HasForeignKey(uob => uob.TrelloUserBoardId);

        modelBuilder.Entity<Table>()
            .HasOne(bl => bl.TrelloUserBoard)
            .WithMany(b => b.Tables)
            .HasForeignKey(bl => bl.BoardId);
    }
}