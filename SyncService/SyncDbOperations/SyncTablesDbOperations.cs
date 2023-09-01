using Microsoft.EntityFrameworkCore;
using Open.Linq.AsyncExtensions;
using TelegramToTrello.ToFromTrello;

namespace TelegramToTrello.SyncDbOperations;

public class SyncTablesDbOperations
{
    private readonly TrelloOperations _trelloOperations;
    private readonly IBoardRepository _boardRepository;
    private readonly ITableRepository _tableRepository;

    public SyncTablesDbOperations(TrelloOperations trelloOperations, IBoardRepository boardRepository, ITableRepository tableRepository)
    {
        _trelloOperations = trelloOperations;
        _boardRepository = boardRepository;
        _tableRepository = tableRepository;
    }
    
    internal async Task Execute(User trelloUser)
    {
        var (currentBoards, currentTables) = await GetCurrentBoardsAndTablesFromDb(trelloUser);
        var freshTableLists = await GetTablesFromTrello(currentBoards, trelloUser);
        await AddNewTablesToDb(freshTableLists, currentTables, currentBoards);
        await RemoveTablesNotInTrello(freshTableLists, currentTables);
    }
    
    private async Task<Dictionary<string, TrelloOperations.TrelloBoardTable>> GetTablesFromTrello(
        Dictionary<string, Board> currentBoards, User trelloUser)
    {
        List<Task<List<TrelloOperations.TrelloBoardTable>>> fetchFreshTablesTask = new();
            
        foreach (var board in currentBoards.Values)
        {
            fetchFreshTablesTask.Add(_trelloOperations.GetBoardTables(board.TrelloBoardId,trelloUser));
        }

        var freshTableLists = await Task.WhenAll(fetchFreshTablesTask);
        var freshTablesMap = freshTableLists.SelectMany(list => list).ToDictionary(t => t.Id);
        
        return freshTablesMap;
    }

    private async Task<(Dictionary<string, Board>, Dictionary<string, Table>)> GetCurrentBoardsAndTablesFromDb(User trelloUser)
    {
        var currentBoards = await _boardRepository.GetAll()
                .Where(b => b.Users.Any(u => u.TelegramId == trelloUser.TelegramId))
                .ToDictionary(b => b.TrelloBoardId, b=> b);

        var currentTables = await _tableRepository.GetAll()
                .Where(bt => bt.TrelloUserBoard.Users.Any(u => u.TelegramId == trelloUser.TelegramId))
                .ToDictionary(t => t.TableId);

            return (currentBoards, currentTables);
    }

    private async Task AddNewTablesToDb(Dictionary<string, TrelloOperations.TrelloBoardTable> freshTableLists,
        Dictionary<string, Table> currentTables, Dictionary<string, Board> currentBoards)
    {
        string[] newTablesArray = freshTableLists.Keys.Except(currentTables.Keys).ToArray();
        if (newTablesArray.Any())
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
                await _tableRepository.AddRange(newTablesList);
        }
    }

    private async Task RemoveTablesNotInTrello(Dictionary<string, TrelloOperations.TrelloBoardTable> freshTableLists,
        Dictionary<string, Table> currentTables)
    {
        string[] tablesToRemoveArray = currentTables.Keys.Except(freshTableLists.Keys).ToArray();
        if (tablesToRemoveArray.Any())
        {
            List<Table> tablesToRemoveList = new();
            foreach (var key in tablesToRemoveArray)
            {
                tablesToRemoveList.Add(currentTables[key]);
            }

            await _tableRepository.RemoveRange(tablesToRemoveList);
        }
    }
}