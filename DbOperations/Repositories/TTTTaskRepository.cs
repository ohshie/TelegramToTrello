using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello.Repositories;

public class TTTTaskRepository : ITTTTaskRepository
{
    private readonly BotDbContext _dbContext;

    public TTTTaskRepository(BotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TTTTask> Get(int id)
    {
        return await _dbContext.CreatingTasks.FindAsync(id);
    }
    
    public async Task<TTTTask> Get(string name)
    {
        return await _dbContext.CreatingTasks
                .FirstOrDefaultAsync(t => t.TaskName == name);
    }
    

    public async Task<IEnumerable<TTTTask>> GetAll()
    {
        return await _dbContext.CreatingTasks.ToListAsync();
    }

    public async Task Add(TTTTask entity)
    {
        await _dbContext.CreatingTasks.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Update(TTTTask entity)
    {
        _dbContext.CreatingTasks.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Delete(TTTTask entity)
    {
        _dbContext.CreatingTasks.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> CheckIfExist(int userId)
    {
        return await _dbContext.CreatingTasks.AnyAsync(ct => ct.Id == userId);
    }
}