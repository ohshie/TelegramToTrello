using Elsa.Activities.Temporal;
using Elsa.Builders;
using NodaTime;
using TelegramToTrello.SyncDbOperations;

namespace TelegramToTrello;

public class SyncService : IWorkflow
{
    private readonly IClock _clock;

    public SyncService(UserDbOperations dbOperations, 
        SyncBoardDbOperations boardDbOperations, 
        SyncTablesDbOperations tablesDbOperations, 
        SyncUsersDbOperations userDbOperations, IClock clock)
    {
        _dbOperations = dbOperations;
        _boardDbOperations = boardDbOperations;
        _tablesDbOperations = tablesDbOperations;
        _userDbOperations = userDbOperations;
        _clock = clock;
    }

    private readonly UserDbOperations _dbOperations;
    private readonly SyncBoardDbOperations _boardDbOperations;
    private readonly SyncTablesDbOperations _tablesDbOperations;
    private readonly SyncUsersDbOperations _userDbOperations;

    private async Task SynchronizeDataToTrello()
    {
        var allUsersList = await _dbOperations.FetchAllUsers();
        
        foreach (var user in allUsersList)
        {
            await SyncStateToTrello(user);
            Console.WriteLine($"{user.TrelloName} synced");
        }
    }
    
    public async Task<bool> SyncStateToTrello(User user)
    {
        if (user != null)
        {
            if (user.TrelloId == string.Empty) return false;
            
            await SyncProcessor(user);
        }
        return true;
    }

    private async Task SyncProcessor(User user)
    {
        await _boardDbOperations.Execute(user);
        await _tablesDbOperations.Execute(user);
        await _userDbOperations.Execute(user);
    }

    public void Build(IWorkflowBuilder builder) =>
        builder
            .AsSingleton()
            .Timer(Duration.FromMinutes(Configuration.SyncTimer).Plus(Duration.FromSeconds(30)))
            .Then(SynchronizeDataToTrello);
}