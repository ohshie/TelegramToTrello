using Microsoft.EntityFrameworkCore;
using Open.Linq.AsyncExtensions;
using TelegramToTrello.ToFromTrello;

namespace TelegramToTrello.SyncDbOperations;

public class SyncUsersDbOperations
{
    private readonly TrelloOperations _trelloOperations;
    private readonly IBoardRepository _boardRepository;
    private readonly ITrelloUsersRepository _userRepository;

    public SyncUsersDbOperations(TrelloOperations trelloOperations, IBoardRepository boardRepository, ITrelloUsersRepository userRepository)
    {
        _trelloOperations = trelloOperations;
        _boardRepository = boardRepository;
        _userRepository = userRepository;
    }
    
    internal async Task Execute(RegisteredUser trelloUser)
    {
        var (currentBoards, currentUsers) = await GetCurrentBoardsAndUsersFromDb(trelloUser);
        var freshUsers = await GetUsersFromTrello(currentBoards, trelloUser);
        await AddNewUsersToDb(freshUsers, currentBoards, currentUsers);
        await RemoveUsersThatAreNotInTrello(freshUsers, currentBoards, currentUsers);
    }
    
    private async Task<(Dictionary<string, Board> currentBoards, HashSet<(string userId, string userName, string boardId)> currentUsers)> GetCurrentBoardsAndUsersFromDb(RegisteredUser trelloUser)
    {
        var currentBoards = await _boardRepository
                .GetAll()
                .Where(b => b.Users.Any(u => u.TelegramId == trelloUser.TelegramId))
                .ToDictionary(b => b.TrelloBoardId);

        var currentUsers = new HashSet<(string userId, string userName, string boardId)>(
            await _userRepository
                .GetAll()
                .Where(uob => uob.TrelloBoard.Users.Any(u => u.TelegramId == trelloUser.TelegramId))
                .Select(uob => (uob.TrelloUserId, uob.Name, uob.TrelloBoard.TrelloBoardId)));

            return (currentBoards, currentUsers);
    }
    
    private async Task<HashSet<(string userId, string userName,string boardId)>> GetUsersFromTrello(
        Dictionary<string, Board> currentBoards, RegisteredUser trelloUser)
    {
        List<Task<List<TrelloOperations.TrelloBoardUser>>> fetchingUsersOnBoardsTasks = new();

        foreach (var board in currentBoards.Values)
        {
            fetchingUsersOnBoardsTasks.Add(_trelloOperations.GetUsersOnBoard(board.TrelloBoardId,trelloUser));
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

            await _userRepository.AddRange(newUsersList);
        }
    }

    private async Task RemoveUsersThatAreNotInTrello(
        HashSet<(string userId, string userName, string boardId)> freshUsersList,
        Dictionary<string, Board> currentBoards, HashSet<(string userId, string userName, string boardId)> currentUsers)
    {
        HashSet<(string id, string name,string boardId)> usersToRemove = currentUsers.Except(freshUsersList).ToHashSet();
        if (usersToRemove.Any())
        {
            List<UsersOnBoard> userToRemoveList = new();
            foreach (var user in usersToRemove)
            {
                Board? board = currentBoards.Values.FirstOrDefault(cb => cb.TrelloBoardId == user.boardId);
                var userToRemove = await _userRepository.GetByTrelloIdAndBoardId(user.id, board.TrelloBoardId);
                userToRemoveList.Add(userToRemove);
            }

            await _userRepository.RemoveRange(userToRemoveList);
        }
    }
}