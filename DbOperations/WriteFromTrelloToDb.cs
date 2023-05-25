using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello;

public class WriteFromTrelloToDb
{
    public async Task PopulateDbWithBoardsUsersTables(RegisteredUser? trelloUser)
    {
        if (trelloUser == null) return;
       
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
                List<Board> newBoardsList = new();
                foreach (var key in newEntries)
                {
                    Board newBoard = new Board
                    {
                        TrelloBoardId = boardsFoundInTrello.GetValueOrDefault(key)!.Id,
                        BoardName = boardsFoundInTrello.GetValueOrDefault(key)!.Name
                    };
                    newBoardsList.Add(newBoard);
                }
                dbContext.Boards.AddRange(newBoardsList);
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
                List<Board> boardToRemoveList = new();
                foreach (var key in entriesToRemove)
                {
                    Board boardToRemove = currentBoardsInDb.GetValueOrDefault(key);
                    boardToRemoveList.Add(boardToRemove);
                }
                dbContext.Boards.RemoveRange(boardToRemoveList);
                await dbContext.SaveChangesAsync();
            }
        }
    }
    
    private async Task CreateUsersBoardsRelations(RegisteredUser trelloUser)
    {
        using (BotDbContext dbContext = new())
        {
            var usersBoardsMap = dbContext.UsersBoards.ToDictionary(ub => ub.BoardId);
            
            List<UsersBoards> usersBoardsList = new();
            foreach (var board in dbContext.Boards)
            {
                if (!usersBoardsMap.ContainsKey(board.Id))
                {
                    var usersBoards = new UsersBoards
                    {
                        UserId = trelloUser.TelegramId,
                        BoardId = board.Id
                    };
                    usersBoardsList.Add(usersBoards);
                }
            }
            dbContext.UsersBoards.AddRange(usersBoardsList);
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
                List<Table> newTablesList = new();
                foreach (var key in newTables)
                {
                    Board? board = currentBoards.Values
                        .FirstOrDefault(cb => cb.TrelloBoardId == freshTableLists.GetValueOrDefault(key)!.BoardId);

                    var newTable = new Table
                    {
                        Name = freshTableLists.GetValueOrDefault(key)!.Name,
                        TableId = freshTableLists.GetValueOrDefault(key)!.Id,
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
        Dictionary<string, Table> currentTables, Dictionary<string, Board> currentBoards)
    {
        var tablesToRemove = currentTables.Keys.Except(currentTables.Keys);
        if (tablesToRemove.Any())
        {
            List<Table> tablesToRemoveList = new();
            using (BotDbContext dbContext = new())
            {
                foreach (var key in tablesToRemove)
                {
                    tablesToRemoveList.Add(currentTables.GetValueOrDefault(key));
                }
                dbContext.BoardTables.RemoveRange(tablesToRemoveList);
                await dbContext.SaveChangesAsync();
            }
        }
    }

    private async Task PopulateBoardsWithUsers(RegisteredUser trelloUser)
    {
        var (currentBoards, currentUsers) = GetCurrentBoardsAndUsersFromDb(trelloUser);
        var freshUsers = await GetUsersFromTrello(currentBoards, trelloUser);
        await AddNewUsersToDb(freshUsers, currentBoards, currentUsers);
        await RemoveUsersThatAreNotInTrello(freshUsers, currentBoards, currentUsers);
    }

    // helpers for PopulateBoardsWithUsers

    private (Dictionary<string, Board> currentBoards, HashSet<(string userId, string userName ,string boardId)> currentUsers)
        GetCurrentBoardsAndUsersFromDb(RegisteredUser trelloUser)
    {
        using (BotDbContext dbContext = new())
        {
            var currentBoards = dbContext.Boards
                .Include(b => b.UsersBoards)
                .Where(b => b.UsersBoards.Any(ub => ub.UserId == trelloUser.TelegramId))
                .ToDictionary(b => b.TrelloBoardId);

            var currentUsers = new HashSet<(string userId, string userName, string boardId)>(dbContext.UsersOnBoards
                .AsEnumerable()
                .Select(uob => (uob.TrelloUserId, uob.Name ,uob.TrelloBoard.TrelloBoardId)));
            
            return (currentBoards, currentUsers);
        }
    }
    
    private async Task<HashSet<(string userId, string userName,string boardId)>> GetUsersFromTrello(
        Dictionary<string, Board> currentBoards, RegisteredUser trelloUser)
    {
        TrelloOperations trelloOperation = new();
            
        List<Task<List<TrelloOperations.TrelloBoardUser>>> fetchingUsersOnBoardsTasks = new();

        foreach (var board in currentBoards.Values)
        {
            fetchingUsersOnBoardsTasks.Add(trelloOperation.GetUsersOnBoard(board.TrelloBoardId,trelloUser));
        }

        var freshUsersLists = await Task.WhenAll(fetchingUsersOnBoardsTasks);

        var freshUsers = freshUsersLists
            .SelectMany(list => list)
            .Select(user => (user.Id, user.Name, user.BoardId))
            .ToHashSet();
        
        return freshUsers;
    }

    private async Task AddNewUsersToDb(HashSet<(string userId, string userName,string boardId)> freshUsersList,
        Dictionary<string, Board> currentBoards, HashSet<(string userId, string userName,string boardId)> currentUsers)
    {
        HashSet<(string userId, string userName, string boardId)> newUsers = freshUsersList.Except(currentUsers).ToHashSet();

        if (newUsers.Any())
        {
            using (BotDbContext dbContext = new())
            {
                List<UsersOnBoard> newUsersList = new();
                
                foreach (var user in newUsers)
                {
                    Board? board = currentBoards.Values.FirstOrDefault(cb => cb.TrelloBoardId == user.boardId);

                    var newUser = new UsersOnBoard
                    {
                        Name = user.userName,
                        TrelloUserId = user.userId,
                        TrelloUserBoardId = board.Id,
                    };
                    
                    newUsersList.Add(newUser);
                }
                dbContext.UsersOnBoards.AddRange(newUsersList);
                await dbContext.SaveChangesAsync();
            } 
        }
    }

    private async Task RemoveUsersThatAreNotInTrello(
        HashSet<(string userId, string userName, string boardId)> freshUsersList,
        Dictionary<string, Board> currentBoards, HashSet<(string userId, string userName, string boardId)> currentUsers)
    {
        HashSet<(string id, string name,string boardId)> usersToRemove = currentUsers.Except(freshUsersList).ToHashSet();
        if (usersToRemove.Any())
        {
            using (BotDbContext dbContext = new())
            {
                List<UsersOnBoard> userToRemoveList = new();
                foreach (var user in usersToRemove)
                {
                    Board? board = currentBoards.Values.FirstOrDefault(cb => cb.TrelloBoardId == user.boardId);
                    var userToRemove = dbContext.UsersOnBoards.FirstOrDefault(u => u.TrelloUserId == user.id && u.TrelloBoard == board);
                    userToRemoveList.Add(userToRemove);
                }
                
                dbContext.UsersOnBoards.RemoveRange(userToRemoveList);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}