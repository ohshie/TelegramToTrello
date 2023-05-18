using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello.Dboperations;

public class WriteFromTrelloToDb
{
    public async Task PopulateDbWithBoardsUsersTables(RegisteredUser trelloUser)
    {
        await CreateBoards(trelloUser);
        await CreateUsersBoardsRelations(trelloUser);
        await PopulateBoardWithTables(trelloUser);
        await PopulateBoardsWithUsers(trelloUser);
    }
    
    private async Task CreateBoards(RegisteredUser trelloUser)
    {
        TrelloOperations trelloOperation = new TrelloOperations();
        var boardsFoundInTrello =
            await trelloOperation.GetTrelloBoards(trelloUser);
        
        await AddNewBoards(boardsFoundInTrello);
        await RemoveBoardThatWereNotInTrello(boardsFoundInTrello);
    }

    private async Task AddNewBoards(Dictionary<string, TrelloOperations.TrelloUserBoard> boardsFoundInTrello)
    {
        using (BotDbContext dbContext = new())
        {
            var currentBoardsInDb = dbContext.Boards.ToDictionary(b => b.TrelloBoardId);

            var newEntries = boardsFoundInTrello.Keys.Except(currentBoardsInDb.Keys);
            if (newEntries.Any())
            {
                foreach (var key in newEntries)
                {
                    dbContext.Boards.Add(new Board
                    {
                        TrelloBoardId = boardsFoundInTrello.GetValueOrDefault(key)!.Id,
                        BoardName = boardsFoundInTrello.GetValueOrDefault(key)!.Name
                    });
                }

                await dbContext.SaveChangesAsync();
            }
        }
    }

    private async Task RemoveBoardThatWereNotInTrello(
        Dictionary<string, TrelloOperations.TrelloUserBoard> boardsFoundInTrello)
    {
        using (BotDbContext dbContext = new BotDbContext())
        {
            var currentBoardsInDb = dbContext.Boards.ToDictionary(b => b.TrelloBoardId);

            var entriesToRemove = currentBoardsInDb.Keys.Except(boardsFoundInTrello.Keys);
            if (entriesToRemove.Any())
            {
                foreach (var key in entriesToRemove)
                {
                    dbContext.Boards.Remove(currentBoardsInDb.GetValueOrDefault(key)!);
                }

                await dbContext.SaveChangesAsync();
            }
        }
    }
    
    private async Task CreateUsersBoardsRelations(RegisteredUser trelloUser)
    {
        using (BotDbContext dbContext = new())
        {
            var usersBoardsMap = dbContext.UsersBoards.ToDictionary(ub => ub.BoardId);

            foreach (var board in dbContext.Boards)
            {
                if (!usersBoardsMap.ContainsKey(board.Id))
                {
                    var usersBoards = new UsersBoards
                    {
                        UserId = trelloUser.TelegramId,
                        BoardId = board.Id
                    };
                    dbContext.Add(usersBoards);
                }
            }
            await dbContext.SaveChangesAsync();
        }
    }
    
    private async Task PopulateBoardWithTables(RegisteredUser trelloUser)
    {
        var (currentBoards, currentTables) = GetCurrentBoardsAndTablesFromDb(trelloUser);
        var freshTableLists = await GetTablesFromTrello(currentBoards, trelloUser);
        await AddNewTablesToDb(freshTableLists, currentTables, currentBoards);
        await RemoveTablesNotInTrello(freshTableLists, currentTables, currentBoards);
    }

    // helpers for PopulateBoardsWithTables
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
                .Include(b => b.UsersBoards)
                .Include(b => b.Tables)
                .Where(b => b.UsersBoards.Any(ub => ub.UserId == trelloUser.TelegramId))
                .ToDictionary(b => b.TrelloBoardId, b=> b);

            var currentTables = dbContext.BoardTables
                .ToDictionary(t => t.TableId);

            return (currentBoards, currentTables);
        }
    }

    private async Task AddNewTablesToDb(Dictionary<string, TrelloOperations.TrelloBoardTable> freshTableLists,
        Dictionary<string, Table> currentTables, Dictionary<string, Board> currentBoards)
    {
        var newTables = freshTableLists.Keys.Except(currentTables.Keys);
        if (newTables.Any())
        {
            using (BotDbContext dbContext = new())
            {
                foreach (var key in newTables)
                {
                    Board? board = currentBoards.Values
                            .FirstOrDefault(cb => cb.TrelloBoardId == freshTableLists.GetValueOrDefault(key)!.BoardId);

                        dbContext.BoardTables.Add(new Table
                        {
                            Name = freshTableLists.GetValueOrDefault(key)!.Name,
                            TableId = freshTableLists.GetValueOrDefault(key)!.Id,
                            BoardId = board.Id,
                        });
                }
                await dbContext.SaveChangesAsync();
            }
        }
    }

    private async Task RemoveTablesNotInTrello(Dictionary<string, TrelloOperations.TrelloBoardTable> freshTableLists,
        Dictionary<string, Table> currentTables, Dictionary<string, Board> currentBoards)
    {
        var tablesToRemove = currentTables.Keys.Except(currentTables.Keys);
        if (tablesToRemove.Any())
        {
            using (BotDbContext dbContext = new())
            {
                foreach (var key in tablesToRemove)
                { 
                    dbContext.BoardTables.Remove(currentTables.GetValueOrDefault(key)!);
                }
                
                await dbContext.SaveChangesAsync();
            }
        }
    }

    private async Task PopulateBoardsWithUsers(RegisteredUser trelloUser)
    {
        var (currentBoards, currentUsers) = GetCurrentBoardsAndUsersFromDb(trelloUser);
        var freshUsers = await GetUsersFromTrello(currentBoards, trelloUser);
        await AddNewUsersToDb(freshUsers, currentBoards, currentUsers);
    }

    // helpers for PopulateBoardsWithUsers

    private (Dictionary<string, Board> currentBoards, HashSet<(string userId, string boardId)> currentUsers)
        GetCurrentBoardsAndUsersFromDb(RegisteredUser trelloUser)
    {
        using (BotDbContext dbContext = new())
        {
            var currentBoards = dbContext.Boards
                .Include(b => b.UsersBoards)
                .Where(b => b.UsersBoards.Any(ub => ub.UserId == trelloUser.TelegramId))
                .ToDictionary(b => b.TrelloBoardId);

            var currentUsers = new HashSet<(string userId, string boardId)>(dbContext.UsersOnBoards
                .AsEnumerable()
                .Select(uob => (uob.TrelloUserId, uob.TrelloBoard.TrelloBoardId)));
            
            return (currentBoards, currentUsers);
        }
    }
    
    private async Task<List<TrelloOperations.TrelloBoardUser>[]> GetUsersFromTrello(
        Dictionary<string, Board> currentBoards, RegisteredUser trelloUser)
    {
        TrelloOperations trelloOperation = new();
            
        List<Task<List<TrelloOperations.TrelloBoardUser>>> fetchingUsersOnBoardsTasks = new();

        foreach (var board in currentBoards.Values)
        {
            fetchingUsersOnBoardsTasks.Add(trelloOperation.GetUsersOnBoard(board.TrelloBoardId,trelloUser));
        }

        var freshUsers = await Task.WhenAll(fetchingUsersOnBoardsTasks);

        return freshUsers;
    }

    private async Task AddNewUsersToDb(List<TrelloOperations.TrelloBoardUser>[] freshUsersList,
        Dictionary<string, Board> currentBoards, HashSet<(string userId, string boardId)> currentUsers)
    {
        using (BotDbContext dbContext = new())
        {
            foreach (var usersList in freshUsersList)
            {
                foreach (var user in usersList)
                {
                    if (!currentUsers.Contains((user.Id, user.BoardId)))
                    {
                        Board? board = currentBoards.Values.FirstOrDefault(cb => cb.TrelloBoardId == user.BoardId);
                        dbContext.UsersOnBoards.Add(new UsersOnBoard
                        {
                            Name = user.Name,
                            TrelloUserId = user.Id,
                            TrelloUserBoardId = board.Id,
                        });
                        currentUsers.Add((user.Id, board.TrelloBoardId));
                    }
                }
                
            }
            Console.WriteLine("users done");
            await dbContext.SaveChangesAsync();
        }
    }
    
    
}