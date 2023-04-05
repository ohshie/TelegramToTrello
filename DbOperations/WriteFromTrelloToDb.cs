using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello.Dboperations;

public class WriteFromTrelloToDb
{
    private TrelloOperations trelloInfo = new TrelloOperations();
    private BotDbContext dbContext = new BotDbContext();

    public async Task PopulateDbWithBoards(RegisteredUsers trelloUser, int telegramId)
    {
        List<TrelloOperations.TrelloUserBoardsList> boardsFoundInTrello =
            await trelloInfo.GetTrelloBoards(trelloUser.TrelloId);

        foreach (var board in boardsFoundInTrello)
        {
            Boards boardsFoundInDb = await dbContext.Boards.FirstOrDefaultAsync(b =>
                b.TrelloBoardId == board.Id);

            if (boardsFoundInDb == null)
            {
                boardsFoundInDb = new Boards
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

            UsersBoards usersBoards =
                await dbContext.UsersBoards.FirstOrDefaultAsync(ub =>
                    ub.UserId == telegramId && ub.BoardId == boardsFoundInDb.Id);

            if (usersBoards == null)
            {
                usersBoards = new UsersBoards
                {
                    UserId = telegramId,
                    BoardId = boardsFoundInDb.Id
                };
                dbContext.Add(usersBoards);
                await dbContext.SaveChangesAsync();
            }

            await dbContext.SaveChangesAsync();
            
            await PopulateBoardWithTables(boardsFoundInDb);
            await PopulateBoardWithUsers(boardsFoundInDb);
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task PopulateBoardWithTables(Boards board)
    {
        List<TrelloOperations.TrelloBoardTablesList> tablesFoundOnBoard =
            await trelloInfo.GetBoardTables(board.TrelloBoardId);

        foreach (var table in tablesFoundOnBoard)
        {
            Tables tablesFoundInDb = await dbContext.BoardTables.FirstOrDefaultAsync(bt =>
                bt.TrelloUserBoard.TrelloBoardId == board.TrelloBoardId && bt.Name == table.Name);

            if (tablesFoundInDb == null)
            {
                tablesFoundInDb = new Tables
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
        }

        await dbContext.SaveChangesAsync();
        Console.WriteLine("tables done");
    }

    private async Task PopulateBoardWithUsers(Boards board)
    {
        List<TrelloOperations.TrelloBoardUsersList> usersFoundOnBoardInTrello =
            await trelloInfo.GetUsersOnBoard(board.TrelloBoardId);

        foreach (var user in usersFoundOnBoardInTrello)
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
        }

        await dbContext.SaveChangesAsync();
        Console.WriteLine("users done");
    }
}