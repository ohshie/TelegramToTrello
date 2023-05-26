using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello;

public class WriteFromTrelloToDb
{
    public async Task PopulateDbWithBoardsUsersTables(RegisteredUser? trelloUser)
    {
        if (trelloUser == null) return;
       
        await CreateBoards(trelloUser);
        await PopulateBoardWithTables(trelloUser);
        await PopulateBoardsWithUsers(trelloUser);
    }
    
    private async Task CreateBoards(RegisteredUser trelloUser)
    {
        TrelloOperations trelloOperation = new TrelloOperations();
        var boardsFoundInTrello =
            await trelloOperation.GetTrelloBoards(trelloUser);
        
        await AddNewBoards(boardsFoundInTrello, trelloUser);
        await RemoveBoardThatWereNotInTrello(boardsFoundInTrello, trelloUser);
    }

    private async Task AddNewBoards(Dictionary<string, TrelloOperations.TrelloUserBoard> boardsFoundInTrello, RegisteredUser trelloUser)
    {
        using (BotDbContext dbContext = new())
        {
            var trackedUser = dbContext.Users
                .Include(u => u.Boards)
                .Single(u => u.TelegramId == trelloUser.TelegramId);
            
            var currentBoardsInDb = dbContext.Boards
                .Include(b => b.Users)
                .ToDictionary(b => b.TrelloBoardId);
            
                Board board;
                foreach (var key in boardsFoundInTrello.Keys)
                {
                    if (currentBoardsInDb.TryGetValue(key, out var existingBoard))
                    {
                        board = existingBoard;
                    }
                    else
                    {
                        board = new Board
                        {
                            TrelloBoardId = boardsFoundInTrello.GetValueOrDefault(key)!.Id,
                            BoardName = boardsFoundInTrello.GetValueOrDefault(key)!.Name,
                        };
                        dbContext.Boards.Add(board);
                    }

                    if (!trackedUser.Boards.Any(b => b.TrelloBoardId == board.TrelloBoardId))
                    {
                        trackedUser.Boards.Add(board);
                        board.Users.Add(trackedUser); 
                    }
                }
                await dbContext.SaveChangesAsync();
        }
    }

    private async Task RemoveBoardThatWereNotInTrello(
        Dictionary<string, TrelloOperations.TrelloUserBoard> boardsFoundInTrello, RegisteredUser trelloUser)
    {
        using (BotDbContext dbContext = new BotDbContext())
        {
            var currentBoardsInDb = dbContext.Boards
                .Where(b => b.Users.Any(u => u.TelegramId== trelloUser.TelegramId))
                .ToDictionary(b => b.TrelloBoardId);

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
    
    private async Task PopulateBoardWithTables(RegisteredUser trelloUser)
    {
        var (currentBoards, currentTables) = GetCurrentBoardsAndTablesFromDb(trelloUser);
        var freshTableLists = await GetTablesFromTrello(currentBoards, trelloUser);
        await AddNewTablesToDb(freshTableLists, currentTables, currentBoards);
        await RemoveTablesNotInTrello(freshTableLists, currentTables);
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
        Dictionary<string, Table> currentTables)
    {
        var tablesToRemove = currentTables.Keys.Except(freshTableLists.Keys);
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
                .Include(b => b.UsersOnBoards)
                .Include(b => b.Users)
                .Where(b => b.Users.Any(u => u.TelegramId == trelloUser.TelegramId))
                .ToDictionary(b => b.TrelloBoardId);

            var currentUsers = new HashSet<(string userId, string userName, string boardId)>
            (dbContext.UsersOnBoards
                .Include(uob => uob.TrelloBoard)
                .Where(uob => uob.TrelloBoard.Users.Any(u => u.TelegramId == trelloUser.TelegramId) )
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