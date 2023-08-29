using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello.Repositories;

public class TrelloUsersRepository : ITrelloUsersRepository
{
    private readonly BotDbContext _botDbContext;

    public TrelloUsersRepository(BotDbContext botDbContext)
    {
        _botDbContext = botDbContext;
    }

    public async Task<UsersOnBoard> Get(int id)
    {
            return await _botDbContext.UsersOnBoards.
                    Include(uob => uob.TrelloBoard)
                    .FirstOrDefaultAsync(uob => uob.Id == id);
    }

    public async Task<UsersOnBoard> Get(string id)
    {  
            return await _botDbContext.UsersOnBoards
                .Include(b => b.TrelloBoard)
                .FirstOrDefaultAsync(u => u.TrelloUserId == id);
    }

    public async Task<IEnumerable<UsersOnBoard>> GetAll()
    { 
            return await _botDbContext.UsersOnBoards.ToListAsync();
    }

    public async Task Add(UsersOnBoard entity)
    {
            await _botDbContext.UsersOnBoards.AddAsync(entity);
            await _botDbContext.SaveChangesAsync();
    }

    public async Task Update(UsersOnBoard entity)
    {
            _botDbContext.UsersOnBoards.Update(entity);
            await _botDbContext.SaveChangesAsync();
    }

    public async Task Delete(UsersOnBoard entity)
    { 
            _botDbContext.UsersOnBoards.Remove(entity); 
            await _botDbContext.SaveChangesAsync();
    }

    public async Task<UsersOnBoard> GetByNameAndBoardId(string name, string boardId)
    { 
            return await _botDbContext.UsersOnBoards
                .Include(b => b.TrelloBoard)
                .FirstOrDefaultAsync(u => u.Name == name 
                                          && u.TrelloBoard.TrelloBoardId == boardId);
    }

    public async Task AddRange(IEnumerable<UsersOnBoard> entity)
    {
            _botDbContext.UsersOnBoards.AddRange(entity);
            await _botDbContext.SaveChangesAsync();
    }

    public async Task RemoveRange(IEnumerable<UsersOnBoard> entity)
    {
            _botDbContext.UsersOnBoards.RemoveRange(entity);
            await _botDbContext.SaveChangesAsync();
    }

    public async Task<UsersOnBoard> GetByTrelloIdAndBoardId(string id, string boardId)
    {
            return await _botDbContext.UsersOnBoards.FirstOrDefaultAsync(uob =>
                    uob.TrelloUserId == id && uob.TrelloBoard.TrelloBoardId == boardId);
    }
}