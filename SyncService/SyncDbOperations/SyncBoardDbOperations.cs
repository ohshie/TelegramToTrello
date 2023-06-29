using Microsoft.EntityFrameworkCore;
using TelegramToTrello.ToFromTrello;

namespace TelegramToTrello.SyncDbOperations;

public class SyncBoardDbOperations
{
    private readonly TrelloOperations _trelloOperations;
    private readonly BotDbContext _botDbContext;

    public SyncBoardDbOperations(TrelloOperations trelloOperations, BotDbContext botDbContext)
    {
        _trelloOperations = trelloOperations;
        _botDbContext = botDbContext;
    }
    
    internal async Task Execute(RegisteredUser user)
    {
        var boardsFoundInTrello =
            await _trelloOperations.GetTrelloBoards(user);

        if (boardsFoundInTrello != null)
        {
            await AddNewBoards(boardsFoundInTrello, user);
            await RemoveBoardThatWereNotInTrello(boardsFoundInTrello, user);
        }
    }
    
    private async Task AddNewBoards(Dictionary<string, TrelloOperations.TrelloUserBoard> boardsFoundInTrello, RegisteredUser trelloUser)
    {
        var (user, currentBoardsInDb) = GetUserAndUserBoards(trelloUser);
            
            foreach (var keyBoardPair in boardsFoundInTrello)
            {
                var board = HandleBoard(currentBoardsInDb, keyBoardPair);
                CreateBoardUserRelations(user, board);
            }
            
            await _botDbContext.SaveChangesAsync();
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
        KeyValuePair<string, TrelloOperations.TrelloUserBoard> keyBoardPair)
    {
        if (currentBoardsInDb.TryGetValue(keyBoardPair.Key, out var existingBoard)) return existingBoard;
      
        Board board = new Board
        {
                TrelloBoardId = keyBoardPair.Value.Id,
                BoardName = keyBoardPair.Value.Name, 
        };
        
        _botDbContext.Boards.Add(board);

        return board;
    }

    private (RegisteredUser trackedUser, Dictionary<string, Board> currentBoardsInDb) GetUserAndUserBoards(RegisteredUser trelloUser)
    {
        RegisteredUser trackedUser = _botDbContext.Users
            .Include(u => u.Boards)
            .Single(u => u.TelegramId == trelloUser.TelegramId);

        Dictionary<string,Board> currentBoardsInDb = _botDbContext.Boards
            .Include(b => b.Users)
            .ToDictionary(b => b.TrelloBoardId!);
        
        return (trackedUser, currentBoardsInDb);
    }

    private async Task RemoveBoardThatWereNotInTrello(
        Dictionary<string, TrelloOperations.TrelloUserBoard> boardsFoundInTrello, RegisteredUser trelloUser)
    {
        var currentBoardsInDb = _botDbContext.Boards
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
            _botDbContext.Boards.RemoveRange(boardToRemoveList);
            await _botDbContext.SaveChangesAsync();
        }
    }
}