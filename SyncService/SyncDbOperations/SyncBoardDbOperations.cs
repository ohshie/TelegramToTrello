using Microsoft.EntityFrameworkCore;
using TelegramToTrello.ToFromTrello;

namespace TelegramToTrello.SyncDbOperations;

internal class SyncBoardDbOperations
{
    internal async Task Execute(RegisteredUser user)
    {
        TrelloOperations trelloOperation = new TrelloOperations();
        var boardsFoundInTrello =
            await trelloOperation.GetTrelloBoards(user);

        if (boardsFoundInTrello != null)
        {
            await AddNewBoards(boardsFoundInTrello, user);
            await RemoveBoardThatWereNotInTrello(boardsFoundInTrello, user);
        }
    }
    
    private async Task AddNewBoards(Dictionary<string, TrelloOperations.TrelloUserBoard> boardsFoundInTrello, RegisteredUser trelloUser)
    {
        using (BotDbContext dbContext = new())
        {
            var (user, currentBoardsInDb) = GetUserAndUserBoards(trelloUser, dbContext);
            
            foreach (var keyBoardPair in boardsFoundInTrello)
            {
                var board = HandleBoard(currentBoardsInDb, keyBoardPair, dbContext);
                CreateBoardUserRelations(user, board);
            }
            
            await dbContext.SaveChangesAsync();
        }
    }

    private void CreateBoardUserRelations(RegisteredUser user, Board board)
    {
        if (!user.Boards.Any(b => b.TrelloBoardId == board.TrelloBoardId))
        {
            user.Boards.Add(board);
            board.Users?.Add(user);
        }
    }

    private Board HandleBoard(Dictionary<string, Board> currentBoardsInDb, 
        KeyValuePair<string, TrelloOperations.TrelloUserBoard> keyBoardPair, 
        BotDbContext dbContext)
    {
        if (currentBoardsInDb.TryGetValue(keyBoardPair.Key, out var existingBoard)) return existingBoard;
      
        Board board = new Board
        {
                TrelloBoardId = keyBoardPair.Value.Id,
                BoardName = keyBoardPair.Value.Name, 
        };
        
        dbContext.Boards.Add(board);

        return board;
    }

    private (RegisteredUser trackedUser, Dictionary<string, Board> currentBoardsInDb) GetUserAndUserBoards(RegisteredUser trelloUser,
        BotDbContext dbContext)
    {
        RegisteredUser trackedUser = dbContext.Users
            .Include(u => u.Boards)
            .Single(u => u.TelegramId == trelloUser.TelegramId);

        Dictionary<string,Board> currentBoardsInDb = dbContext.Boards
            .Include(b => b.Users)
            .ToDictionary(b => b.TrelloBoardId!);
        
        return (trackedUser, currentBoardsInDb);
    }

    private async Task RemoveBoardThatWereNotInTrello(
        Dictionary<string, TrelloOperations.TrelloUserBoard> boardsFoundInTrello, RegisteredUser trelloUser)
    {
        await using BotDbContext dbContext = new BotDbContext();
        var currentBoardsInDb = dbContext.Boards
            .Where(b => b.Users.Any(u => u.TelegramId== trelloUser.TelegramId))
            .ToDictionary(b => b.TrelloBoardId!);

        string[] entriesToRemove = currentBoardsInDb.Keys.Except(boardsFoundInTrello.Keys).ToArray();
        
        if (entriesToRemove.Any())
        {
            List<Board> boardToRemoveList = new();
            foreach (var key in entriesToRemove)
            {
                Board? boardToRemove = currentBoardsInDb.GetValueOrDefault(key);
                boardToRemoveList.Add(boardToRemove);
            }
            dbContext.Boards.RemoveRange(boardToRemoveList);
            await dbContext.SaveChangesAsync();
        }
    }
}