using TelegramToTrello.SyncDbOperations;

namespace TelegramToTrello;

public class SyncService
{
    public SyncService(UserDbOperations dbOperations, 
        SyncBoardDbOperations boardDbOperations, 
        SyncTablesDbOperations tablesDbOperations, 
        SyncUsersDbOperations userDbOperations)
    {
        _dbOperations = dbOperations;
        _boardDbOperations = boardDbOperations;
        _tablesDbOperations = tablesDbOperations;
        _userDbOperations = userDbOperations;
    }

    private readonly UserDbOperations _dbOperations;
    private readonly SyncBoardDbOperations _boardDbOperations;
    private readonly SyncTablesDbOperations _tablesDbOperations;
    private readonly SyncUsersDbOperations _userDbOperations;

    public async Task SynchronizeDataToTrello()
    {
        var allUsersList = await _dbOperations.FetchAllUsers();
        
        foreach (var user in allUsersList)
        {
            await SyncStateToTrello(user);
            Console.WriteLine($"{user.TrelloName} synced");
        }
    }
    
    public async Task<bool> SyncStateToTrello(RegisteredUser user)
    {
        if (user != null)
        {
            if (user.TrelloId == string.Empty) return false;
            
            await SyncProcessor(user);
        }
        return true;
    }

    private async Task SyncProcessor(RegisteredUser user)
    {
        await _boardDbOperations.Execute(user);
        await _tablesDbOperations.Execute(user);
        await _userDbOperations.Execute(user);
    }
}