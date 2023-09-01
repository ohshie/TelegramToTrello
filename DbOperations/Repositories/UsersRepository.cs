using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly BotDbContext _botDbContext;

    public UsersRepository(BotDbContext botDbContext)
    {
        _botDbContext = botDbContext;
    }

    public async Task<User> Get(int id)
    {
        return await _botDbContext.Users.FirstOrDefaultAsync(u => u.TelegramId == id);
    }

    public async Task<User> Get(string id)
    {
        return await _botDbContext.Users.FirstOrDefaultAsync(u => u.TrelloId == id);
    }

    public async Task<IEnumerable<User>> GetAll()
    {
        return await _botDbContext.Users
                .Include(u => u.Boards)
                .ToListAsync();
    }

    public async Task Add(User entity)
    {
        _botDbContext.Users.Add(entity);
        await _botDbContext.SaveChangesAsync();
    }

    public async Task Update(User entity)
    {
        _botDbContext.Users.Update(entity);
        await _botDbContext.SaveChangesAsync();
    }

    public async Task Delete(User entity)
    {
        _botDbContext.Users.Remove(entity);
        await _botDbContext.SaveChangesAsync();
    }

    public async Task<User> GetUserWithBoards(int id)
    { 
        return await _botDbContext.Users
                .Include(ru => ru.Boards)
                .FirstOrDefaultAsync(um => um.TelegramId == id);
    }

    public async Task<bool> CheckExist(int id)
    {
        return await _botDbContext.Users.AnyAsync(u => u.TelegramId == id);
    }
}