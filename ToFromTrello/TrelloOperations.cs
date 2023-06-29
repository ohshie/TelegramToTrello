using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello.ToFromTrello;

public class TrelloOperations
{
    private readonly HttpClient _httpClient;
    private readonly BotDbContext _botDbContext;

    public TrelloOperations(HttpClient httpClient, BotDbContext botDbContext)
    {
        _httpClient = httpClient;
        _botDbContext = botDbContext;
        
        _trelloApiKey = Configuration.TrelloKey;
    }
    
    private readonly string? _trelloApiKey;

    public async Task<string?> GetTrelloUserId(string token)
    {
        HttpResponseMessage responseMessage = (await _httpClient.GetAsync(
            $"https://api.trello.com/1/members/me?key={_trelloApiKey}&token={token}"));

        if (responseMessage.IsSuccessStatusCode)
        {
            string content = await responseMessage.Content.ReadAsStringAsync();
            JsonDocument json = JsonDocument.Parse(content);
            
            if (json.RootElement.GetProperty("id").GetString() == null)
            {
                return null;
            }

            return json.RootElement.GetProperty("id").GetString();
        }

        return null;
    }
    
    public async Task<Dictionary<string, TrelloUserBoard>?> GetTrelloBoards(RegisteredUser userName)
    {
        HttpResponseMessage response = (await _httpClient.GetAsync(
            $"https://api.trello.com/1/members/{userName.TrelloId}/boards?key={_trelloApiKey}&token={userName.TrelloToken}"));

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Dictionary<string, TrelloUserBoard>? boardsMap = JsonSerializer.Deserialize<List<TrelloUserBoard>>(content)?.ToDictionary(b => b.Id);
            return boardsMap;
        }

        return null;
    }

    public async Task<List<TrelloBoardTable>?> GetBoardTables(string boardName, RegisteredUser trelloUser)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"https://api.trello.com/1/boards/{boardName}/lists?key={_trelloApiKey}&token={trelloUser.TrelloToken}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TrelloBoardTable>>(content);
        }

        return null;
    }

    public async Task<List<TrelloBoardUser>?> GetUsersOnBoard(string boardId, RegisteredUser trelloUser)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"https://api.trello.com/1/boards/{boardId}/members?key={_trelloApiKey}&token={trelloUser.TrelloToken}");

        if (!response.IsSuccessStatusCode) return null;
        
        var content = await response.Content.ReadAsStringAsync();
        List<TrelloBoardUser>? newUsers = JsonSerializer.Deserialize<List<TrelloBoardUser>>(content);

        if (newUsers == null) return null;
        
        foreach (var user in newUsers)
        {
            user.BoardId = boardId;
        }
        return newUsers;
    }
    
    public async Task<bool> PushTaskToTrello(TTTTask task)
    {
        RegisteredUser? trelloUser = await _botDbContext.Users.FindAsync(task.Id);
            string trelloApiUri = $"https://api.trello.com/1/cards";

            string correctDate = DateTime.Parse(task.Date!).AddHours(-4).ToString("o");
            string participants = $"{task.TaskPartId}{task.TrelloId}";
            string combinedTaskNameAndTag = $"[{task.Tag}] {task.TaskName}";
            
            var requestUri = $"{trelloApiUri}?key={_trelloApiKey}" +
                             $"&token={trelloUser?.TrelloToken}" +
                             $"&name={Uri.EscapeDataString(combinedTaskNameAndTag)}" +
                             $"&idList={task.ListId}" +
                             $"&idMembers={Uri.EscapeDataString(participants)}" +
                             $"&due={Uri.EscapeDataString(correctDate)}" +
                             $"&desc={Uri.EscapeDataString(task.TaskDesc!)}";

            HttpResponseMessage response = await _httpClient.PostAsync(requestUri, null);

            if (await AddTaskToNotifications(task, response, correctDate, trelloUser)) return true;

            Console.WriteLine($"failed {response}");
            return false;
    }

    private async Task<bool> AddTaskToNotifications(TTTTask task, HttpResponseMessage response,
        string correctDate, RegisteredUser? trelloUser)
    {
        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);
            JsonDocument trelloResponse = JsonDocument.Parse(content);
            string? taskId = trelloResponse.RootElement.GetProperty("id").GetString();
            string? taskUrl = trelloResponse.RootElement.GetProperty("shortUrl").GetString();
            string? tableName = await _botDbContext.BoardTables
                .Where(bt => bt.TableId == task.ListId)
                .Select(bt => bt.Name)
                .FirstOrDefaultAsync();
            
            if (taskId != null)
            {
                _botDbContext.TaskNotifications.Add(new TaskNotification
                {
                    TaskId = taskId,
                    TaskListId = task.ListId,
                    TaskList = tableName,
                    TaskBoard = task.TrelloBoardName,
                    TaskBoardId = task.TrelloBoardId,
                    Due = correctDate,
                    Name = task.TaskName,
                    Description = task.TaskDesc,
                    Url = taskUrl,
                    User = trelloUser!.TelegramId
                });
                await _botDbContext.SaveChangesAsync();
            }

            return true;
        }

        return false;
    }

    public async Task<bool> MarkTaskAsComplete(string taskId, RegisteredUser user)
    {
        string requestUrl = $"https://api.trello.com/1/cards/{taskId}?key={_trelloApiKey}" +
                            $"&token={user.TrelloToken}" +
                            $"&dueComplete=true" +
                            $"&subscribed=false";

        HttpResponseMessage responseMessage = await _httpClient.PutAsync(requestUrl, null);
            
        if (responseMessage.IsSuccessStatusCode)
        {
            Console.WriteLine(responseMessage);
            return true;
        }

        Console.WriteLine(responseMessage +"failed");
        return false;
    }
    
    private async Task<List<TrelloCard>?> FetchCardsFromTrello(RegisteredUser user)
    {
        string query = Uri.EscapeDataString("@me is:open has:members created:month sort:-edited");
        string cardLimit = "50";

        string cardsUrl =
                $"https://api.trello.com/1/search?" +
                $"query={query}&" +
                $"key={_trelloApiKey}&" +
                $"token={user.TrelloToken}&" +
                $"idBoards=mine&" +
                $"cards_limit={cardLimit}";
            
        var cardsResponse = await _httpClient.GetAsync(cardsUrl);

        if (!cardsResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to fetch cards for board");
            return null;
        }
        
        string cardsJson = await cardsResponse.Content.ReadAsStringAsync();
        TrelloSearchResponse? searchResponse = JsonSerializer.Deserialize<TrelloSearchResponse>(cardsJson);
        List<TrelloCard>? cards = searchResponse?.CardItems;

        return cards;
    }

    public async Task<Dictionary<string, TrelloCard>?> GetSubscribedTasks(RegisteredUser user)
    {
        List<TrelloCard>? cards = await FetchCardsFromTrello(user);
        if (cards != null)
        {
            DateTime minDueDate = DateTime.UtcNow.AddDays(-7);
            
            Dictionary<string, TrelloCard> filteredCards = cards.Where(card => card.Due != null &&
                                                                               DateTime.Parse(card.Due) >= minDueDate &&
                                                                               card.SubscribeStatus && !card.Status).ToDictionary(c => c.Id);
            
            return filteredCards;
        }
        return null;
    }

    // helper classes to create a list of trello boards/lists/users for selected user
    public class TrelloUserBoard
    {
        [JsonPropertyName("id")] 
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("name")] 
        public string Name { get; set; } = string.Empty;
    }
    
    public class TrelloBoardTable
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("name")] 
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("idBoard")]
        public string BoardId { get; set; } = string.Empty;
    }

    public class TrelloBoardUser
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("fullName")]
        public string Name { get; set; } = string.Empty;
        
        public string? BoardId { get; set; } 
    }

    public class TrelloSearchResponse
    {
        [JsonPropertyName("cards")]
        public List<TrelloCard>? CardItems { get; set; }
    }
    
    public class TrelloCard
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("due")]
        public string? Due { get; set; } = string.Empty;
        [JsonPropertyName("idMembers")]
        public string[]? Members { get; set; } 
        [JsonPropertyName("subscribed")] 
        public bool SubscribeStatus { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("desc")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("shortUrl")]
        public string Url { get; set; } = string.Empty;
        [JsonPropertyName("dueComplete")]
        public bool Status { get; set; }
        [JsonPropertyName("idBoard")]
        public string? BoardId { get; set; }
        [JsonPropertyName("idList")]
        public string? ListId { get; set; }
    }
}