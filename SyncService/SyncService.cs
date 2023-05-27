using TelegramToTrello.SyncDbOperations;

namespace TelegramToTrello;

public class SyncService
{
    private readonly UserDbOperations _dbOperations = new();
    private readonly SyncBoardDbOperations _boardDbOperations = new();
    private readonly SyncTablesDbOperations _tablesDbOperations = new();
    private readonly SyncUsersDbOperations _userDbOperations = new();

    public async Task SynchronizeDataToTrello()
    {
        var allUsersList = await _dbOperations.FetchAllUsers();
        
        foreach (var user in allUsersList)
        {
            await SyncStateToTrello(user);
            Console.WriteLine("synced");
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