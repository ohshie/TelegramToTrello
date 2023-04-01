using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello.Dboperations;

public class WriteFromTrelloToDb
{
    private TrelloOperations trelloInfo = new TrelloOperations();
    private BotDbContext dbContext = new BotDbContext();

    public async Task PopulateDbWithBoards(TrelloUser trelloUser, int telegramId)
    {
        List<TrelloOperations.TrelloUserBoardsList> boardsFoundInTrello =
            await trelloInfo.GetTrelloBoards(trelloUser.TrelloId);

        foreach (var board in boardsFoundInTrello)
        {
            TrelloUserBoard boardsFoundInDb = await dbContext.TrelloUserBoards.FirstOrDefaultAsync(tub =>
                tub.TrelloBoardId == board.Id && tub.TrelloUserId == trelloUser.TrelloId);

            if (boardsFoundInDb == null)
            {
                boardsFoundInDb = new TrelloUserBoard
                {
                    TrelloBoardId = board.Id,
                    Name = board.Name,
                    TrelloUserId = trelloUser.TrelloId,
                    TelegramId = telegramId
                };
                dbContext.TrelloUserBoards.Add(boardsFoundInDb);
            }
            else
            {
                boardsFoundInDb.Name = board.Name;
                boardsFoundInDb.TrelloBoardId = board.Id;
            }

            await dbContext.SaveChangesAsync();

            await PopulateBoardWithTables(boardsFoundInDb);

            await PopulateBoardWithUsers(boardsFoundInDb);
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task PopulateBoardWithTables(TrelloUserBoard board)
    {
        List<TrelloOperations.TrelloBoardTablesList> tablesFoundOnBoard =
            await trelloInfo.GetBoardTables(board.TrelloBoardId);

        foreach (var table in tablesFoundOnBoard)
        {
            TrelloBoardTable tablesFoundInDb = await dbContext.BoardTables.FirstOrDefaultAsync(bt =>
                bt.TrelloUserBoard.TrelloBoardId == board.TrelloBoardId && bt.Name == table.Name);

            if (tablesFoundInDb == null)
            {
                tablesFoundInDb = new TrelloBoardTable
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
    }

    private async Task PopulateBoardWithUsers(TrelloUserBoard board)
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
    }
}