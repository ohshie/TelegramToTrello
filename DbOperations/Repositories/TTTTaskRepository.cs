using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello;

public class TTTTaskRepository : IRepository<TTTTask>
{
    public async Task<TTTTask> Get(int id)
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.CreatingTasks.FindAsync(id);
        }
    }

    /// <summary>
    /// Attempt to get task by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<TTTTask> Get(string name)
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.CreatingTasks
                .FirstOrDefaultAsync(t => t.TaskName == name);
        }
    }
    

    public async Task<IEnumerable<TTTTask>> GetAll()
    {
        using (BotDbContext dbContext = new())
        {
            return await dbContext.CreatingTasks.ToListAsync();
        }
    }

    public async Task Add(TTTTask entity)
    {
        using (BotDbContext dbContext = new())
        {
            await dbContext.CreatingTasks.AddAsync(entity);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task Update(TTTTask entity)
    {
        using (BotDbContext dbContext = new())
        {
            dbContext.CreatingTasks.Update(entity);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task Delete(TTTTask entity)
    {
        using (BotDbContext dbContext = new())
        {
            dbContext.CreatingTasks.Remove(entity);
            await dbContext.SaveChangesAsync();   
        }
    }
}