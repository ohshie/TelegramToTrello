using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramToTrello;

public class TrelloOperations
{
    private static readonly string TrelloApiKey = Environment.GetEnvironmentVariable("Trello_API_Key");

    public async Task<string> GetTrelloUserId(string token)
    {
        using HttpClient httpClient = new HttpClient();
        HttpResponseMessage responseMessage = (await httpClient.GetAsync(
            $"https://api.trello.com/1/members/me?key={TrelloApiKey}&token={token}"));

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
    
    public async Task<List<TrelloUserBoardsList>> GetTrelloBoards(RegisteredUser userName)
    {
        using HttpClient httpClient = new HttpClient();
        HttpResponseMessage response = (await httpClient.GetAsync(
            $"https://api.trello.com/1/members/{userName.TrelloId}/boards?key={TrelloApiKey}&token={userName.TrelloToken}"));

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TrelloUserBoardsList>>(content);
        }

        return null;
    }

    public async Task<List<TrelloBoardTablesList>> GetBoardTables(string boardName, RegisteredUser trelloUser)
    {
        using HttpClient httpClient = new HttpClient();
        HttpResponseMessage response = await httpClient.GetAsync($"https://api.trello.com/1/boards/{boardName}/lists?key={TrelloApiKey}&token={trelloUser.TrelloToken}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TrelloBoardTablesList>>(content);
        }

        return null;
    }

    public async Task<List<TrelloBoardUsersList>> GetUsersOnBoard(string boardName, RegisteredUser trelloUser)
    {
        using HttpClient httpClient = new HttpClient();
        HttpResponseMessage response =
            await httpClient.GetAsync(
                $"https://api.trello.com/1/boards/{boardName}/members?key={TrelloApiKey}&token={trelloUser.TrelloToken}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TrelloBoardUsersList>>(content);
        }

        return null;
    }
    
    public async Task<bool> PushTaskToTrello(TTTTask task)
    {
        using (BotDbContext dbContext = new BotDbContext())
        using (HttpClient httpClient = new HttpClient())
        {
            RegisteredUser trelloUser = await dbContext.Users.FindAsync(task.Id);
            string trelloApiUri = $"https://api.trello.com/1/cards";

            string participants = task.TaskPartId.Remove(task.TaskPartId.Length-1);
            string combinedTaskNameAndTag = $"[{task.Tag}] {task.TaskName}";
            var requestUri = $"{trelloApiUri}?key={TrelloApiKey}" +
                             $"&token={trelloUser.TrelloToken}" +
                             $"&name={Uri.EscapeDataString(combinedTaskNameAndTag)}" +
                             $"&idList={task.ListId}" +
                             $"&idMembers={Uri.EscapeDataString(participants)}" +
                             $"&due={Uri.EscapeDataString(task.Date)}" +
                             $"&desc={Uri.EscapeDataString(task.TaskDesc)}";

            HttpResponseMessage response = await httpClient.PostAsync(requestUri, null);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
                return true;
            }

            Console.WriteLine($"failed {response}");
            return false;
        }
    }

    private async Task<bool> PushDateToTrello(TTTTask userCreatedTask)
    {
        using (BotDbContext dbContext = new BotDbContext())
        using (HttpClient httpClient = new HttpClient())
        {
            RegisteredUser trelloUser = await dbContext.Users.FindAsync(userCreatedTask.Id);
            string trelloApiUri = $"https://api.trello.com/1/cards/{userCreatedTask.TaskId}?due={userCreatedTask.Date}&" +
                                  $"key={TrelloApiKey}&token={trelloUser.TrelloToken}";
            
            HttpResponseMessage response = await httpClient.PutAsync(trelloApiUri, null);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
                return true;
            }

            Console.WriteLine($"failed {response}");
            return false;
        }
    }

    public async Task<List<TrelloCards>> GetCardsOnBoards(RegisteredUser user)
    {
        using HttpClient httpClient = new HttpClient();
        {
            string isOpenFilter = "is:open";
            string cardLimit = "150";
            
            string cardsUrl =
                $"https://api.trello.com/1/search?"+
                $"query={isOpenFilter}&" +
                $"key={TrelloApiKey}&" +
                $"token={user.TrelloToken}&" +
                $"idBoards=mine&" +
                $"cards_limit={cardLimit}";
            
            HttpResponseMessage cardsResponse = await httpClient.GetAsync(cardsUrl);
            
            if (!cardsResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch cards for board");
                return null;
            }

            string cardsJson = await cardsResponse.Content.ReadAsStringAsync();
            TrelloSearchResponse searchResponse = JsonSerializer.Deserialize<TrelloSearchResponse>(cardsJson);
            List<TrelloCards> cards = searchResponse.CardItems;
            
            DateTime minDueDate = DateTime.UtcNow.AddDays(-7);
            DateTime maxDueDate = DateTime.UtcNow.AddDays(9999);
            
            List<TrelloCards> filteredCards = cards.Where(card => card.Due != null &&
                                                           DateTime.Parse(card.Due) >= minDueDate &&
                                                           DateTime.Parse(card.Due) <= maxDueDate &&
                                                           card.Members.Contains(user.TrelloId)).ToList();
            
            return filteredCards;
        }
        
    }

    // helper classes to create a list of trello boards/lists/users for selected user
    public class TrelloUserBoardsList
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("name")] 
        public string Name { get; set; }
    }
    
    public class TrelloBoardTablesList
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("name")] 
        public string Name { get; set; }
    }

    public class TrelloBoardUsersList
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("fullName")]
        public string Name { get; set; }
    }

    public class TrelloSearchResponse
    {
        [JsonPropertyName("cards")]
        public List<TrelloCards> CardItems { get; set; }
    }
    
    public class TrelloCards
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("due")]
        public string? Due { get; set; }
        [JsonPropertyName("idMembers")]
        public string[] Members { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("desc")]
        public string Description { get; set; }
        [JsonPropertyName("shortUrl")]
        public string Url { get; set; }
        [JsonPropertyName("closed")]
        public bool Status { get; set; }
    }
}