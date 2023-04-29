using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello.Dboperations;

public class WriteFromTrelloToDb
{
    private TrelloOperations trelloInfo = new TrelloOperations();

    public async Task PopulateDbWithBoardsUsersTables(RegisteredUser trelloUser)
    {
        List<TrelloOperations.TrelloUserBoardsList> boardsFoundInTrello =
            await trelloInfo.GetTrelloBoards(trelloUser);
        
        List<List<TrelloOperations.TrelloBoardTablesList>> allListsFromUser =
            new List<List<TrelloOperations.TrelloBoardTablesList>>();

        List<List<TrelloOperations.TrelloBoardUsersList>> allUsersOnAllBoardsFromUser =
            new List<List<TrelloOperations.TrelloBoardUsersList>>();

        List<Board> boardsFoundInDb = new List<Board>();
        
        foreach (var board in boardsFoundInTrello)
        {
            Board boardToList = await GerOrCreateBoards(board);
            boardsFoundInDb.Add(boardToList);
        }

        foreach (var board in boardsFoundInDb)
        {
            await GetOrCreateUsersBoards(trelloUser, board);
            
            await PopulateBoardWithTables(board, trelloUser);
            await PopulateBoardWithUsers(board, trelloUser);
        }
    }


    private async Task<Board> GerOrCreateBoards(TrelloOperations.TrelloUserBoardsList board)
    {
        using (BotDbContext dbContext = new BotDbContext())
        {
            Board boardsFoundInDb = await dbContext.Boards.FirstOrDefaultAsync(b =>
                b.TrelloBoardId == board.Id);
        
            if (boardsFoundInDb == null)
            {
                boardsFoundInDb = new Board
                {
                    TrelloBoardId = board.Id,
                    BoardName = board.Name,
                };
                dbContext.Boards.Add(boardsFoundInDb);
                await dbContext.SaveChangesAsync();
            }
            else
            {
                boardsFoundInDb.BoardName = board.Name;
                boardsFoundInDb.TrelloBoardId = board.Id;
            }

            return boardsFoundInDb;
        }
    }

    private async Task GetOrCreateUsersBoards(RegisteredUser trelloUser, Board board)
    {
        using (BotDbContext dbContext = new BotDbContext())
        {
            UsersBoards usersBoards =
                await dbContext.UsersBoards.FirstOrDefaultAsync(ub =>
                    ub.UserId == trelloUser.TelegramId && ub.BoardId == board.Id);
        
            if (usersBoards == null)
            {
                usersBoards = new UsersBoards
                {
                    UserId = trelloUser.TelegramId,
                    BoardId = board.Id
                };
                dbContext.Add(usersBoards);
                await dbContext.SaveChangesAsync();
            }
        }
    }
    
    private async Task PopulateBoardWithTables(Board board, RegisteredUser trelloUser)
    {
        List<TrelloOperations.TrelloBoardTablesList> tablesFoundOnBoard =
            await trelloInfo.GetBoardTables(board.TrelloBoardId, trelloUser);
        
        foreach (var table in tablesFoundOnBoard)
        {
            using (BotDbContext dbContext = new BotDbContext())
            {
                Table tablesFoundInDb = await dbContext.BoardTables.FirstOrDefaultAsync(bt =>
                    bt.TrelloUserBoard.TrelloBoardId == board.TrelloBoardId && bt.Name == table.Name);

                if (tablesFoundInDb == null)
                {
                    tablesFoundInDb = new Table
                    {
                        Name = table.Name,
                        TableId = table.Id,
                        BoardId = board.Id
                    };
                    dbContext.BoardTables.Add(tablesFoundInDb);
                }
                else
                {
                    tablesFoundInDb.Name = table.Name;
                    tablesFoundInDb.TableId = table.Id;
                }
                await dbContext.SaveChangesAsync();
            }
            
        }
        Console.WriteLine("table done");
    }

    private async Task PopulateBoardWithUsers(Board board, RegisteredUser trelloUser)
    {
        List<TrelloOperations.TrelloBoardUsersList> usersFoundOnBoardInTrello =
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