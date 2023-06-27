using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello;

public class TrelloUsersRepository : IRepository<UsersOnBoard>, ITrelloUsersRepository
{
    public async Task<UsersOnBoard> Get(int id)
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.UsersOnBoards.FindAsync(id);
        }
    }

    public async Task<UsersOnBoard> Get(string id)
    {   
        using (BotDbContext dbContext = new())
        {
            return await dbContext.UsersOnBoards
                .Include(b => b.TrelloBoard)
                .FirstOrDefaultAsync(u => u.TrelloUserId == id);
        }
    }

    public async Task<IEnumerable<UsersOnBoard>> GetAll()
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.UsersOnBoards.ToListAsync();
        }
    }

    public async Task Add(UsersOnBoard entity)
    {
        using (BotDbContext dbContext = new())
        {
            await dbContext.UsersOnBoards.AddAsync(entity);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task Update(UsersOnBoard entity)
    {
        using (BotDbContext dbContext = new())
        {
            dbContext.UsersOnBoards.Update(entity);
            await dbContext.SaveChangesAsync();   
        }
    }

    public async Task Delete(UsersOnBoard entity)
    {
        using (BotDbContext dbContext = new())
        {
            dbContext.UsersOnBoards.Remove(entity);
            await dbContext.SaveChangesAsync();   
        }
    }

    public async Task<UsersOnBoard> GetByNameAndBoardId(string name, string boardId)
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.UsersOnBoards
                .Include(b => b.TrelloBoard)
                .FirstOrDefaultAsync(u => u.Name == name 
                                          && u.TrelloBoard.TrelloBoardId == boardId);
        }
    }
}