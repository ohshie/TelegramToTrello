using TelegramToTrello.SyncDbOperations;

namespace TelegramToTrello;

public class SyncService
{
    private readonly UserDbOperations _dbOperations = new();

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

            WriteFromTrelloToDb writeFromTrelloToDb = new WriteFromTrelloToDb();
            await writeFromTrelloToDb.PopulateDbWithBoardsUsersTables(user);
        }
        return true;
    }

    private async Task SyncProcessor(RegisteredUser user)
    {
        SyncBoardDbOperations boardDbOperations = new();
        await boardDbOperations.CreateBoards(user);
    }
}