using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello.Dboperations;

public class WriteFromTrelloToDb
{
    private readonly TrelloOperations _trelloInfo = new();

    public async Task PopulateDbWithBoardsUsersTables(RegisteredUser trelloUser)
    {
        List<TrelloOperations.TrelloUserBoard> boardsFoundInTrello =
            await _trelloInfo.GetTrelloBoards(trelloUser);

        await CreateBoards(boardsFoundInTrello);
        await CreateUsersBoardsRelations(trelloUser);
        await PopulateBoardWithTables(trelloUser);
        await PopulateBoardsWithUsers(trelloUser);
    }

    private async Task CreateBoards(List<TrelloOperations.TrelloUserBoard> boardsFoundInTrello)
    {
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
        using (BotDbContext dbContext = new())
        {
            var currentBoards = dbContext.Boards
                .Include(b => b.UsersBoards)
                .Include(b => b.Tables)
                .Where(b => b.UsersBoards.Any(ub => ub.UserId == trelloUser.TelegramId))
                .ToDictionary(b => b.TrelloBoardId, b=> b);

            var currentTables = dbContext.BoardTables
                .ToDictionary(t => t.TableId);

            List<Task<List<TrelloOperations.TrelloBoardTable>>> fetchFreshTablesTask = new();
            
            foreach (var board in currentBoards.Values)
            {
                fetchFreshTablesTask.Add(_trelloInfo.GetBoardTables(board.TrelloBoardId,trelloUser));
            }

            var freshTableListTest = await Task.WhenAll(fetchFreshTablesTask);
            
            foreach (var tableListOnBoard in freshTableListTest)
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
        using (BotDbContext dbContext = new())
        {
            var currentBoards = dbContext.Boards
                .Include(b => b.UsersBoards)
                .Where(b => b.UsersBoards.Any(ub => ub.UserId == trelloUser.TelegramId))
                .ToDictionary(b => b.TrelloBoardId);

            var currentUsers = new HashSet<(string userId, string boardId)>(dbContext.UsersOnBoards
                .AsEnumerable()
                .Select(uob => (uob.TrelloUserId, uob.TrelloBoard.TrelloBoardId)));

            List<Task<List<TrelloOperations.TrelloBoardUser>>> fetchingUsersOnBoardsTasks = new();

            foreach (var board in currentBoards.Values)
            {
                fetchingUsersOnBoardsTasks.Add(_trelloInfo.GetUsersOnBoard(board.TrelloBoardId,trelloUser));
            }

            var freshUsers = await Task.WhenAll(fetchingUsersOnBoardsTasks);
            
            foreach (var usersList in freshUsers)
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