using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello.Repositories;

public class TemplateRepository : ITemplateRepository
{
    private readonly BotDbContext _dbContext;

    public TemplateRepository(BotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Template> Get(int id)
    {
        return await _dbContext.Templates.FindAsync(id);
    }

    public async Task<Template> Get(string id)
    {
        return await _dbContext.Templates.FindAsync(id);
    }

    public async Task<IEnumerable<Template>> GetAll()
    {
        return await _dbContext.Templates.ToListAsync();
    }

    public async Task Add(Template entity)
    {
        _dbContext.Templates.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Update(Template entity)
    {
        _dbContext.Templates.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Delete(Template entity)
    {
        _dbContext.Templates.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<Template>> GetAllTemplatesFromUser(int id)
    {
        return await _dbContext.Templates
            .Where(t => t.UserId == id)
            .ToListAsync();
    }

    public async Task<List<Template>> GetAllTemplatesByUserAndBoard(int userId, string boardId)
    {
        return await _dbContext.Templates
            .Where(t => t.UserId == userId && t.BoardId == boardId && t.Complete)
            .ToListAsync();
    }
    
    public async Task<Template> GetIncompleteTemplate(int id)
    {
        return await _dbContext.Templates
            .Where(t => !t.Complete && t.UserId == id)
            .FirstOrDefaultAsync();
    }
}