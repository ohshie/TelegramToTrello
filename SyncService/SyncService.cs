namespace TelegramToTrello;

public class SyncService
{
    private DbOperations _dbOperations = new();

    public async Task SynchronizeDataToTrello()
    {
        var allUsersList = await _dbOperations.FetchAllUsers();
        
        foreach (var user in allUsersList)
        {
            await _dbOperations.LinkBoardsFromTrello(user.TelegramId);
            Console.WriteLine("synced");
        }
    }
}