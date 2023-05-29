using Microsoft.EntityFrameworkCore;
using TelegramToTrello.ToFromTrello;

namespace TelegramToTrello.SyncDbOperations;

internal class SyncTablesDbOperations
{
    internal async Task Execute(RegisteredUser trelloUser)
    {
        var (currentBoards, currentTables) = GetCurrentBoardsAndTablesFromDb(trelloUser);
        var freshTableLists = await GetTablesFromTrello(currentBoards, trelloUser);
        await AddNewTablesToDb(freshTableLists, currentTables, currentBoards);
        await RemoveTablesNotInTrello(freshTableLists, currentTables);
    }
    
    private async Task<Dictionary<string, TrelloOperations.TrelloBoardTable>> GetTablesFromTrello(
        Dictionary<string, Board> currentBoards, RegisteredUser trelloUser)
    {
        TrelloOperations trelloOperation = new TrelloOperations();
        
        List<Task<List<TrelloOperations.TrelloBoardTable>>> fetchFreshTablesTask = new();
            
        foreach (var board in currentBoards.Values)
        {
            fetchFreshTablesTask.Add(trelloOperation.GetBoardTables(board.TrelloBoardId,trelloUser));
        }

        var freshTableLists = await Task.WhenAll(fetchFreshTablesTask);
        var freshTablesMap = freshTableLists.SelectMany(list => list).ToDictionary(t => t.Id);
        
        return freshTablesMap;
    }

    private (Dictionary<string, Board>, Dictionary<string, Table>) GetCurrentBoardsAndTablesFromDb(RegisteredUser trelloUser)
    {
        using (BotDbContext dbContext = new())
        {
            var currentBoards = dbContext.Boards
                .Include(b => b.Tables)
                .Include(b => b.Users)
                .Where(b => b.Users.Any(u => u.TelegramId == trelloUser.TelegramId))
                .ToDictionary(b => b.TrelloBoardId, b=> b);

            var currentTables = dbContext.BoardTables
                .Include(bt => bt.TrelloUserBoard)
                .Where(bt => bt.TrelloUserBoard.Users.Any(u => u.TelegramId == trelloUser.TelegramId))
                .ToDictionary(t => t.TableId);

            return (currentBoards, currentTables);
        }
    }

    private async Task AddNewTablesToDb(Dictionary<string, TrelloOperations.TrelloBoardTable> freshTableLists,
        Dictionary<string, Table> currentTables, Dictionary<string, Board> currentBoards)
    {
        string[] newTablesArray = freshTableLists.Keys.Except(currentTables.Keys).ToArray();
        if (newTablesArray.Any())
        {
            using (BotDbContext dbContext = new())
            {
                List<Table> newTablesList = new();
                foreach (var key in newTablesArray)
                {
                    Board? board = currentBoards.Values
                        .FirstOrDefault(cb => cb.TrelloBoardId == freshTableLists[key].BoardId);

                    var newTable = new Table
                    {
                        Name = freshTableLists[key].Name,
                        TableId = freshTableLists[key].Id,
                        BoardId = board.Id,
                    };
                    newTablesList.Add(newTable);
                }
                dbContext.BoardTables.AddRange(newTablesList);
                await dbContext.SaveChangesAsync();
            }
        }
    }

    private async Task RemoveTablesNotInTrello(Dictionary<string, TrelloOperations.TrelloBoardTable> freshTableLists,
        Dictionary<string, Table> currentTables)
    {
        string[] tablesToRemoveArray = currentTables.Keys.Except(freshTableLists.Keys).ToArray();
        if (tablesToRemoveArray.Any())
        {
            List<Table> tablesToRemoveList = new();
            using (BotDbContext dbContext = new())
            {
                foreach (var key in tablesToRemoveArray)
                {
                    tablesToRemoveList.Add(currentTables[key]);
                }
                dbContext.BoardTables.RemoveRange(tablesToRemoveList);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}