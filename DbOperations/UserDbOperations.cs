using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using TelegramToTrello.Repositories;

namespace TelegramToTrello;

public class UserDbOperations
{
    private readonly IUsersRepository _usersRepository;

    public UserDbOperations(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<bool> RegisterNewUser(Message message)
    {
        var userExist = await _usersRepository.Get((int)message.From.Id);
        if (userExist == null)
        {
            userExist = new User
            {
                TelegramId = (int)message.From.Id,
                TelegramName = message.From.Username,
            };

            await _usersRepository.Add(userExist);
            
            return true;
        }

        return false;
    }

    public async Task<User?> AddTrelloTokenAndId(string token, string trelloId, int telegramId)
    {
        var userExist = await _usersRepository.Get(telegramId);
        if (userExist != null)
        {
            userExist.TrelloToken = token;
            userExist.TrelloId = trelloId;

            await _usersRepository.Update(userExist);
            return userExist;
        }

        return userExist;
    }
    
    public async Task<User?> RetrieveTrelloUser(int telegramId)
    {
        var trelloUser = await _usersRepository.GetUserWithBoards(telegramId);
        
        if (trelloUser != null)
        {
            return trelloUser;
        }
        return null;
    }

    public async Task<bool> CheckIfExist(int telegramId)
    {
        return await _usersRepository.CheckExist(telegramId);
    }
    
    public async Task<List<User>> FetchAllUsers()
    {
        var users = await _usersRepository.GetAll();
        var usersList = users.ToList();
        
        return usersList;
    }
}