using ManagementTool.DB;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ManagementTool.Data
{
    public class Managament
    {
        HttpClient _httpClient;

        public Managament(HttpClient client)
        {
            _httpClient = client;
        }

        // Read
        public async Task<List<UserDb>> GetGameResultsAsync()
        {
            var result = await _httpClient.GetAsync("api/usertable");

            var resultContent = await result.Content.ReadAsStringAsync();
            List<UserDb> resGameResults = JsonConvert.DeserializeObject<List<UserDb>>(resultContent);
            return resGameResults;
        }
    }
}
