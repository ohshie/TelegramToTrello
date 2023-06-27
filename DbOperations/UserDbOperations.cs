using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;

namespace TelegramToTrello;

public class UserDbOperations
{
    private UsersRepository _usersRepository = new();
    
    public async Task<bool> RegisterNewUser(Message message)
    {
        var userExist = await _usersRepository.Get((int)message.From.Id);
        if (userExist == null)
        {
            userExist = new RegisteredUser
            {
                TelegramId = (int)message.From.Id,
                TelegramName = message.From.Username,
            };

            await _usersRepository.Add(userExist);
            return true;
        }

        return false;
    }
        // await using BotDbContext dbContext = new BotDbContext();
        // {
        //     RegisteredUser? existingUser = await dbContext.Users.FindAsync((int)message.From!.Id);
        //
        //     if (existingUser == null)
        //     {
        //         dbContext.Users.Add(new RegisteredUser
        //         {
        //             TelegramId = (int)message.From.Id,
        //             TelegramName = message.From.Username,
        //         });
        //         
        //         await dbContext.SaveChangesAsync();
        //         return true;
        //     }
        // }
        // return false;

    public async Task<RegisteredUser?> AddTrelloTokenAndId(string token, string trelloId, int telegramId)
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
        
        // await using BotDbContext dbContext = new BotDbContext();
        // {
        //     RegisteredUser? existingUser = await dbContext.Users.FindAsync(telegramId);
        //
        //     if (existingUser != null)
        //     {
        //         existingUser.TrelloToken = token;
        //         existingUser.TrelloId = trelloId;
        //
        //         await dbContext.SaveChangesAsync();
        //     }
        //
        //     return existingUser;
        // }
    }
    
    public async Task<RegisteredUser?> RetrieveTrelloUser(int telegramId)
    {
        var trelloUser = await _usersRepository.GetUserWithBoards(telegramId);
        
        if (trelloUser != null)
        {
            return trelloUser;
        }
        return null;
        
        // await using (BotDbContext dbContext = new BotDbContext())
        // {
        //     RegisteredUser? trelloUser = await dbContext.Users
        //         .Include(ru => ru.Boards)
        //         .FirstOrDefaultAsync(um => um.TelegramId == telegramId);
        //
        //     if (trelloUser != null)
        //     {
        //         return trelloUser;
        //     }
        // }
        // return null;
    } 
    
    public async Task<List<RegisteredUser>> FetchAllUsers()
    {
        var users = await _usersRepository.GetAll();
        var usersList = users.ToList();
        
        return usersList;
        
        using (BotDbContext dbContext = new())
        {
            return await dbContext.Users
                .Include(u => u.Boards)
                .ToListAsync();
        }
    }
}