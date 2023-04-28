using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello;

public class BotDbContext : DbContext
{
    public DbSet<RegisteredUser> Users { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<TTTTask> CreatingTasks { get; set; }
    public DbSet<Table> BoardTables { get; set; }
    public DbSet<UsersOnBoard> UsersOnBoards { get; set; }
    public DbSet<UsersBoards> UsersBoards { get; set; }
    public DbSet<TaskNotification> TaskNotifications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=postgres;Username=postgres;Password=**");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RegisteredUser>()
            .HasKey(u => u.TelegramId);

        modelBuilder.Entity<Board>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<Table>()
            .HasKey(bl => bl.Id);
        
        modelBuilder.Entity<UsersOnBoard>()
            .HasKey(uob => uob.Id);

        modelBuilder.Entity<UsersBoards>()
            .HasKey(ub => new
            {
                ub.UserId, ub.BoardId
            });

        modelBuilder.Entity<UsersBoards>()
            .HasOne(ub => ub.RegisteredUsers)
            .WithMany(u => u.UsersBoards)
            .HasForeignKey(ub => ub.UserId);
        
        modelBuilder.Entity<UsersBoards>()
            .HasOne(ub => ub.Boards)
            .WithMany(u => u.UsersBoards)
            .HasForeignKey(ub => ub.BoardId);

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