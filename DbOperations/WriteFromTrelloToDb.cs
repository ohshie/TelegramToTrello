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
        List<TrelloOperations.TrelloUserBoard> boardsFoundInTrello =
            await trelloOperation.GetTrelloBoards(trelloUser);
        
        using (BotDbContext dbContext = new())
        {
            var currentBoardsInDb = dbContext.Boards.ToDictionary(b => b.TrelloBoardId);

            foreach (var board in boardsFoundInTrello)
            {
                if (!currentBoardsInDb.ContainsKey(board.Id))
                {
                    dbContext.Boards.Add(new Board
                    {
                        TrelloBoardId = board.Id,
                        BoardName = board.Name
                    });
                }
            }

            await dbContext.SaveChangesAsync();
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
        await AddNewBoardsToDb(freshTableLists, currentTables, currentBoards);
    }

    // helpers for PopulateBoardsWithTables
    private async Task<List<TrelloOperations.TrelloBoardTable>[]> GetTablesFromTrello(
        Dictionary<string, Board> currentBoards, RegisteredUser trelloUser)
    {
        TrelloOperations trelloOperation = new TrelloOperations();
        
        List<Task<List<TrelloOperations.TrelloBoardTable>>> fetchFreshTablesTask = new();
            
        foreach (var board in currentBoards.Values)
        {
            fetchFreshTablesTask.Add(trelloOperation.GetBoardTables(board.TrelloBoardId,trelloUser));
        }

        var freshTableListTest = await Task.WhenAll(fetchFreshTablesTask);
        
        return freshTableListTest;
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

    private async Task AddNewBoardsToDb(List<TrelloOperations.TrelloBoardTable>[] freshTableLists,
        Dictionary<string, Table> currentTables, Dictionary<string, Board> currentBoards)
    {
        using (BotDbContext dbContext = new())
        {
            foreach (var tableListOnBoard in freshTableLists)
            { 
                foreach (var table in tableListOnBoard)
                {
                    if (!currentTables.ContainsKey(table.Id))
                    {
                        Board? board = currentBoards.Values
                            .FirstOrDefault(cb => cb.TrelloBoardId == table.BoardId);

                        dbContext.BoardTables.Add(new Table
                        {
                            Name = table.Name,
                            TableId = table.Id,
                            BoardId = board.Id,
                            TrelloUserBoard = board
                        });
                    }
                }
            }
            Console.WriteLine("table done");
            await dbContext.SaveChangesAsync();
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
                            TrelloBoard = board
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