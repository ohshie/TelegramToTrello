using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello;

public class BotDbContext : DbContext
{
    public DbSet<TrelloUser> TrelloUsers { get; set; }
    public DbSet<TrelloUserBoard> TrelloUserBoards { get; set; }
    public DbSet<TTTTask> CreatingTasks { get; set; }
    public DbSet<TrelloBoardTable> BoardTables { get; set; }
    public DbSet<UsersOnBoard> UsersOnBoards { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=teltotrel.sqlite");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrelloUser>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<TrelloUserBoard>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<TrelloBoardTable>()
            .HasKey(bl => bl.Id);
        
        modelBuilder.Entity<UsersOnBoard>()
            .HasKey(uob => uob.Id);

        modelBuilder.Entity<UsersOnBoard>()
            .HasOne(uob => uob.TrelloBoard)
            .WithMany(tub => tub.UsersOnBoards)
            .HasForeignKey(uob => uob.TrelloUserBoardId);
        
        modelBuilder.Entity<TrelloUserBoard>()
            .HasOne(b => b.TrelloUser)
            .WithMany(u => u.TrelloUserBoards)
            .HasForeignKey(b => b.TelegramId);

        modelBuilder.Entity<TrelloBoardTable>()
            .HasOne(bl => bl.TrelloUserBoard)
            .WithMany(b => b.TrelloBoardTables)
            .HasForeignKey(bl => bl.BoardId);
    }
}