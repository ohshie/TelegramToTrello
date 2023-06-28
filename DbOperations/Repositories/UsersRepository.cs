using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly BotDbContext _botDbContext;

    public UsersRepository(BotDbContext botDbContext)
    {
        _botDbContext = botDbContext;
    }

    public async Task<RegisteredUser> Get(int id)
    {
        return await _botDbContext.Users.FindAsync(id);
    }

    public async Task<RegisteredUser> Get(string id)
    {
        return await _botDbContext.Users.FirstOrDefaultAsync(u => u.TrelloId == id);
    }

    public async Task<IEnumerable<RegisteredUser>> GetAll()
    {
        return await _botDbContext.Users
                .Include(u => u.Boards)
                .ToListAsync();
    }

    public async Task Add(RegisteredUser entity)
    {
        _botDbContext.Users.Add(entity);
        await _botDbContext.SaveChangesAsync();
    }

    public async Task Update(RegisteredUser entity)
    {
        _botDbContext.Users.Update(entity);
        await _botDbContext.SaveChangesAsync();
    }

    public async Task Delete(RegisteredUser entity)
    {
        _botDbContext.Users.Remove(entity);
        await _botDbContext.SaveChangesAsync();
    }

    public async Task<RegisteredUser> GetUserWithBoards(int id)
    { 
        return await _botDbContext.Users
                .Include(ru => ru.Boards)
                .FirstOrDefaultAsync(um => um.TelegramId == id);
    }
}