using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            var content = await responseMessage.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            if (json["id"].ToString() == null) return null;

            return json["id"].ToString();
        }

        return null;
    }
    
    public async Task<List<TrelloUserBoardsList>> GetTrelloBoards(RegisteredUsers userName)
    {
        using HttpClient httpClient = new HttpClient();
        HttpResponseMessage response = (await httpClient.GetAsync(
            $"https://api.trello.com/1/members/{userName.TrelloId}/boards?key={TrelloApiKey}&token={userName.TrelloToken}"));

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<TrelloUserBoardsList>>(content);
        }

        return null;
    }

    public async Task<List<TrelloBoardTablesList>> GetBoardTables(string boardName, RegisteredUsers trelloUser)
    {
        using HttpClient httpClient = new HttpClient();
        HttpResponseMessage response = await httpClient.GetAsync($"https://api.trello.com/1/boards/{boardName}/lists?key={TrelloApiKey}&token={trelloUser.TrelloToken}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<TrelloBoardTablesList>>(content);
        }

        return null;
    }

    public async Task<List<TrelloBoardUsersList>> GetUsersOnBoard(string boardName, RegisteredUsers trelloUser)
    {
        using HttpClient httpClient = new HttpClient();
        HttpResponseMessage response =
            await httpClient.GetAsync(
                $"https://api.trello.com/1/boards/{boardName}/members?key={TrelloApiKey}&token={trelloUser.TrelloToken}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<TrelloBoardUsersList>>(content);
        }

        return null;
    }

    public async Task<string> PushTaskToTrello(TTTTask userCreatedTask)
    {
        using (BotDbContext dbContext = new BotDbContext())
        using (HttpClient httpClient = new HttpClient())
        {
            RegisteredUsers trelloUser = await dbContext.Users.FindAsync(userCreatedTask.Id);
            
            string combinedTagAndTaskName = $"[{userCreatedTask.Tag}] {userCreatedTask.TaskName}";
            string trelloApiUri = $"https://api.trello.com/1/cards?idList={userCreatedTask.ListId}&name={Uri.EscapeDataString(combinedTagAndTaskName)}&key={TrelloApiKey}&token={trelloUser.TrelloToken}";

            HttpResponseMessage response = await httpClient.PostAsync(trelloApiUri, null);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject taskId = JObject.Parse(responseBody);
                if (taskId["id"].ToString() == null) return null;

                return taskId["id"].ToString();
            }
            
            Console.WriteLine($"failed to create card. status code: {response}");
                return null;
        }
    }

    public async Task<bool> PushTaskDescriptionToTrello(TTTTask userCreatedTask)
    {
        using (BotDbContext dbContext = new BotDbContext())
        using (HttpClient httpClient = new HttpClient())
        {
            RegisteredUsers trelloUser = await dbContext.Users.FindAsync(userCreatedTask.Id);
            
            string trelloApiUri = $"https://api.trello.com/1/cards/{userCreatedTask.TaskId}?" +
                                  $"desc={Uri.EscapeDataString(userCreatedTask.TaskDesc)}&key={TrelloApiKey}&token={trelloUser.TrelloToken}";

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

    public async Task<bool> PushTaskParticipantToTrello(TTTTask userCreatedTask)
    {
        using (BotDbContext dbContext = new BotDbContext())
        using (HttpClient httpClient = new HttpClient())
        {
            RegisteredUsers trelloUser = await dbContext.Users.FindAsync(userCreatedTask.Id);
            string trelloApiUri = $"https://api.trello.com/1/cards/{userCreatedTask.TaskId}/idMembers?" +
                                  $"key={TrelloApiKey}&token={trelloUser.TrelloToken}&value={userCreatedTask.TaskCurrentParticipant}";

            HttpResponseMessage response = await httpClient.PostAsync(trelloApiUri, null);

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

    public async Task<bool> PushDateToTrello(TTTTask userCreatedTask)
    {
        using (BotDbContext dbContext = new BotDbContext())
        using (HttpClient httpClient = new HttpClient())
        {
            RegisteredUsers trelloUser = await dbContext.Users.FindAsync(userCreatedTask.Id);
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
    
    // helper classes to create a list of trello boards/lists/users for selected user
    public class TrelloUserBoardsList
    {
        [JsonProperty("Id")]
        public string Id { get; set; }
        
        [JsonProperty("name")] 
        public string Name { get; set; }
    }
    
    public class TrelloBoardTablesList
    {
        [JsonProperty("Id")]
        public string Id { get; set; }
        
        [JsonProperty("name")] 
        public string Name { get; set; }
    }

    public class TrelloBoardUsersList
    {
        [JsonProperty("Id")]
        public string Id { get; set; }
        [JsonProperty("fullname")]
        public string Name { get; set; }
    }
}