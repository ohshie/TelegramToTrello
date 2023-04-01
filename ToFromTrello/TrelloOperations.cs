using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TelegramToTrello;

public class TrelloOperations
{
    static readonly string TrelloAPIKey = Environment.GetEnvironmentVariable("Trello_API_Key");
    static readonly string TrelloToken = Environment.GetEnvironmentVariable("Trello_Token");
    
    public async Task<string> GetTrelloUserIdFromTrelloAPI(string userName)
    {
        using HttpClient httpClient = new HttpClient();
        HttpResponseMessage response = (await httpClient.GetAsync(
            $"https://api.trello.com/1/members/{userName}?key={TrelloAPIKey}&token={TrelloToken}"));
       
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            if (json["id"].ToString() == null) return null;
            
            return json["id"].ToString();
        }

        return null;
    }

    public async Task<List<TrelloUserBoardsList>> GetTrelloBoards(string userName)
    {
        using HttpClient httpClient = new HttpClient();
        HttpResponseMessage response = (await httpClient.GetAsync(
            $"http://api.trello.com/1/members/{userName}/boards?key={TrelloAPIKey}&token={TrelloToken}"));

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<TrelloUserBoardsList>>(content);
        }

        return null;
    }

    public async Task<List<TrelloBoardTablesList>> GetBoardTables(string boardName)
    {
        using HttpClient httpClient = new HttpClient();
        HttpResponseMessage responce = await httpClient.GetAsync($"http://api.trello.com/1/boards/{boardName}/lists?key={TrelloAPIKey}&token={TrelloToken}");

        if (responce.IsSuccessStatusCode)
        {
            var content = await responce.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<TrelloBoardTablesList>>(content);
        }

        return null;
    }

    public async Task<List<TrelloBoardUsersList>> GetUsersOnBoard(string boardName)
    {
        using HttpClient httpClient = new HttpClient();
        HttpResponseMessage response =
            await httpClient.GetAsync(
                $"https://api.trello.com/1/boards/{boardName}/members?key={TrelloAPIKey}&token={TrelloToken}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<TrelloBoardUsersList>>(content);
        }

        return null;
    }

    public async Task<string> PushTaskToTrello(TTTTask userCreatedTask)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            string trelloApiURI = $"https://api.trello.com/1/cards?idList={userCreatedTask.ListId}&name={userCreatedTask.TaskName}&key={TrelloAPIKey}&token={TrelloToken}";

            HttpResponseMessage response = await httpClient.PostAsync(trelloApiURI, null);

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
        using (HttpClient httpClient = new HttpClient())
        {
            string trelloApiUri = $"https://api.trello.com/1/cards/{userCreatedTask.TaskId}?" +
                                  $"desc={Uri.EscapeDataString(userCreatedTask.TaskDesc)}&key={TrelloAPIKey}&token={TrelloToken}";

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
        using (HttpClient httpClient = new HttpClient())
        {
            string trelloApiUri = $"https://api.trello.com/1/cards/{userCreatedTask.TaskId}/idMembers?" +
                                  $"key={TrelloAPIKey}&token={TrelloToken}&value={userCreatedTask.TaskCurrentParticipant}";

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