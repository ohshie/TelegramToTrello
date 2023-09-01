using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello.Repositories;

public class DialogueStorageRepository : IDialogueStorageRepository
{
    private BotDbContext _dbContext;

    public DialogueStorageRepository(BotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DialogueStorage> Get(int id)
    {
        return await _dbContext.DialogueStorages
            .FirstOrDefaultAsync(ds => ds.Id == id);
    }

    public async Task<DialogueStorage> Get(string id)
    {
        return await _dbContext.DialogueStorages
            .FirstOrDefaultAsync(ds => ds.Id == int.Parse(id));
    }

    public async Task<IEnumerable<DialogueStorage>> GetAll()
    {
        return await _dbContext.DialogueStorages.ToListAsync();
    }

    public async Task Add(DialogueStorage entity)
    {
        _dbContext.DialogueStorages.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Update(DialogueStorage entity)
    {
        _dbContext.DialogueStorages.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Delete(DialogueStorage entity)
    {
        _dbContext.DialogueStorages.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }
}