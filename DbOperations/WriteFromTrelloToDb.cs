using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello.Dboperations;

public class WriteFromTrelloToDb
{
    private TrelloOperations trelloInfo = new();

    public async Task PopulateDbWithBoardsUsersTables(RegisteredUser trelloUser)
    {
        List<TrelloOperations.TrelloUserBoard> boardsFoundInTrello =
            await trelloInfo.GetTrelloBoards(trelloUser);
        
        List<Board> boardsFoundInDb = new List<Board>();
        
        
        
        // foreach (var board in boardsFoundInTrello)
        // {
        //     Board boardToList = await GerOrCreateBoards(board);
        //     boardsFoundInDb.Add(boardToList);
        // }

        await CreateBoards(boardsFoundInTrello);
        await CreateUsersBoardsRelations(trelloUser);
        await PopulateBoardWithTables(trelloUser);

        foreach (var board in boardsFoundInDb)
        {
            //await PopulateBoardWithTables(board, trelloUser);
            await PopulateBoardWithUsers(board, trelloUser);
        }
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
    
    // private async Task<Board> GerOrCreateBoards(TrelloOperations.TrelloUserBoard board)
    // {
    //     using (BotDbContext dbContext = new BotDbContext())
    //     {
    //         Board boardsFoundInDb = await dbContext.Boards.FirstOrDefaultAsync(b =>
    //             b.TrelloBoardId == board.Id);
    //     
    //         if (boardsFoundInDb == null)
    //         {
    //             boardsFoundInDb = new Board
    //             {
    //                 TrelloBoardId = board.Id,
    //                 BoardName = board.Name,
    //             };
    //             dbContext.Boards.Add(boardsFoundInDb);
    //             await dbContext.SaveChangesAsync();
    //         }
    //         else
    //         {
    //             boardsFoundInDb.BoardName = board.Name;
    //             boardsFoundInDb.TrelloBoardId = board.Id;
    //         }
    //     
    //         return boardsFoundInDb;
    //     }
    // }

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
    
    // private async Task GetOrCreateUsersBoards(RegisteredUser trelloUser, Board board)
    // {
    //     using (BotDbContext dbContext = new BotDbContext())
    //     {
    //         UsersBoards usersBoards =
    //             await dbContext.UsersBoards.FirstOrDefaultAsync(ub =>
    //                 ub.UserId == trelloUser.TelegramId && ub.BoardId == board.Id);
    //     
    //         if (usersBoards == null)
    //         {
    //             usersBoards = new UsersBoards
    //             {
    //                 UserId = trelloUser.TelegramId,
    //                 BoardId = board.Id
    //             };
    //             dbContext.Add(usersBoards);
    //             await dbContext.SaveChangesAsync();
    //         }
    //     }
    // }

    private async Task PopulateBoardWithTables(RegisteredUser trelloUser)
    {
        using (BotDbContext dbContext = new())
        {
            var currentBoards = dbContext.Boards
                .Include(b => b.UsersBoards)
                .Include(b => b.Tables)
                .Where(b => b.UsersBoards.Any(ub => ub.UserId == trelloUser.TelegramId))
                .ToDictionary(b => b.TrelloBoardId, b=> b);

            var currentTables = dbContext.BoardTables.ToDictionary(t => t.TableId);

            List<Task<List<TrelloOperations.TrelloBoardTable>>> fetchFreshTablesTask = new();
            
            foreach (var board in currentBoards.Values)
            {
                fetchFreshTablesTask.Add(trelloInfo.GetBoardTables(board.TrelloBoardId,trelloUser));
            }

            var freshTableListTest = await Task.WhenAll(fetchFreshTablesTask);

            foreach (var board in currentBoards)
            {
                foreach (var tableListOnBoard in freshTableListTest)
                {
                    foreach (var table in tableListOnBoard)
                    {
                        if (!currentTables.ContainsKey(table.Id))
                        {
                            dbContext.BoardTables.Add(new Table
                            {
                                Name = table.Name,
                                TableId = table.Id,
                                BoardId = board.Value.Id,
                                TrelloUserBoard = board.Value
                            });
                        }
                    }
                }
                Console.WriteLine("table done");
            }
            await dbContext.SaveChangesAsync();
        }
    }
    
    // private async Task PopulateBoardWithTables(Board board, RegisteredUser trelloUser)
    // {
    //     List<TrelloOperations.TrelloBoardTable> tablesFoundOnBoard =
    //         await trelloInfo.GetBoardTables(board.TrelloBoardId, trelloUser);
    //     
    //     foreach (var table in tablesFoundOnBoard)
    //     {
    //         using (BotDbContext dbContext = new BotDbContext())
    //         {
    //             Table tablesFoundInDb = await dbContext.BoardTables.FirstOrDefaultAsync(bt =>
    //                 bt.TrelloUserBoard.TrelloBoardId == board.TrelloBoardId && bt.Name == table.Name);
    //
    //             if (tablesFoundInDb == null)
    //             {
    //                 tablesFoundInDb = new Table
    //                 {
    //                     Name = table.Name,
    //                     TableId = table.Id,
    //                     BoardId = board.Id
    //                 };
    //                 dbContext.BoardTables.Add(tablesFoundInDb);
    //             }
    //             else
    //             {
    //                 tablesFoundInDb.Name = table.Name;
    //                 tablesFoundInDb.TableId = table.Id;
    //             }
    //             await dbContext.SaveChangesAsync();
    //         }
    //         
    //     }
    //     Console.WriteLine("table done");
    // }

    private async Task PopulateBoardWithUsers(Board board, RegisteredUser trelloUser)
    {
        List<TrelloOperations.TrelloBoardUser> usersFoundOnBoardInTrello =
            await trelloInfo.GetUsersOnBoard(board.TrelloBoardId, trelloUser);

        foreach (var user in usersFoundOnBoardInTrello)
        {
            using (BotDbContext dbContext = new BotDbContext())
            {
                UsersOnBoard usersOnBoardFoundInDb = await dbContext.UsersOnBoards.FirstOrDefaultAsync(uob =>
                    uob.TrelloUserId == user.Id && uob.TrelloUserBoardId == board.Id);

                if (usersOnBoardFoundInDb == null)
                {
                    usersOnBoardFoundInDb = new UsersOnBoard
                    {
                        TrelloUserId = user.Id,
                        Name = user.Name,
                        TrelloUserBoardId = board.Id
                    };
                    dbContext.UsersOnBoards.Add(usersOnBoardFoundInDb);
                }
                else
                {
                    usersOnBoardFoundInDb.TrelloUserId = user.Id;
                    usersOnBoardFoundInDb.Name = user.Name;
                }
                await dbContext.SaveChangesAsync();
            }
        }
            
        Console.WriteLine("users done");
    }
}