using Microsoft.EntityFrameworkCore;
using Open.Linq.AsyncExtensions;
using TelegramToTrello.ToFromTrello;

namespace TelegramToTrello.SyncDbOperations;

public class SyncBoardDbOperations
{
    private readonly TrelloOperations _trelloOperations;
    private readonly IBoardRepository _boardRepository;
    private readonly IUsersRepository _usersRepository;

    public SyncBoardDbOperations(TrelloOperations trelloOperations, 
        IBoardRepository boardRepository,
        IUsersRepository usersRepository)
    {
        _trelloOperations = trelloOperations;
        _boardRepository = boardRepository;
        _usersRepository = usersRepository;
    }
    
    internal async Task Execute(User user)
    {
        var boardsFoundInTrello =
            await _trelloOperations.GetTrelloBoards(user);

        if (boardsFoundInTrello != null)
        {
            await AddNewBoards(boardsFoundInTrello, user);
            await RemoveBoardThatWereNotInTrello(boardsFoundInTrello, user);
        }
    }
    
    private async Task AddNewBoards(Dictionary<string, TrelloOperations.TrelloUserBoard> boardsFoundInTrello, User trelloUser)
    {
        var (user, currentBoardsInDb) = await GetUserAndUserBoards(trelloUser);
            
            foreach (var keyBoardPair in boardsFoundInTrello)
            {
                if(currentBoardsInDb.ContainsKey(keyBoardPair.Key)) continue;
                
                var board = HandleBoard(keyBoardPair, user);
                await _boardRepository.Add(board);
            }
    }

    private void CreateBoardUserRelations(User user, Board board)
    {
        if (!user.Boards.Any(b => b.TrelloBoardId == board.TrelloBoardId))
        {
            user.Boards.Add(board);
            board.Users?.Add(user);
        }
    }

    private Board HandleBoard( 
        KeyValuePair<string, TrelloOperations.TrelloUserBoard> keyBoardPair, User user)
    {
        Board board = new Board
        {
                TrelloBoardId = keyBoardPair.Value.Id,
                BoardName = keyBoardPair.Value.Name, 
        };
        
        CreateBoardUserRelations(user, board);

        return board;
    }

    private async Task<(User trackedUser, Dictionary<string, Board> currentBoardsInDb)> GetUserAndUserBoards(User trelloUser)
    {
        User trackedUser = await _usersRepository.GetUserWithBoards(trelloUser.TelegramId);

        Dictionary<string,Board> currentBoardsInDb = await _boardRepository.GetAll()
            .ToDictionary(b => b.TrelloBoardId!);
        
        return (trackedUser, currentBoardsInDb);
    }

    private async Task RemoveBoardThatWereNotInTrello(
        Dictionary<string, TrelloOperations.TrelloUserBoard> boardsFoundInTrello, User trelloUser)
    {
        var currentBoardsInDb = await _boardRepository.GetAll()
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

            await _boardRepository.DeleteRange(boardToRemoveList);
        }
    }
}