using AccountServer.DB;
using Newtonsoft.Json;
using ServerContents.DB;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
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

        #region Account DB Read
        public async Task<List<AccountDB>> GetAccountListAsync()
        {
            var result = await _httpClient.GetAsync("api/account");

            var resultContent = await result.Content.ReadAsStringAsync();
            List<AccountDB> resGameResults = JsonConvert.DeserializeObject<List<AccountDB>>(resultContent);
            return resGameResults;
        }
        #endregion Account DB Read

        #region GameServer DB Read
        // 유저 목록 가져오기
        public async Task<List<UserDb>> GetUserListAsync()
        {
            var result = await _httpClient.GetAsync("api/gameserver/users");
            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<UserDb>>(content);
        }

        // 유저 상세 정보 가져오기
        public async Task<UserDb> GetUserAsync(string userId)
        {
            var result = await _httpClient.GetAsync($"api/gameserver/users/{userId}");
            result.EnsureSuccessStatusCode();

            var content = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UserDb>(content);
        }

        // 유저 정보 업데이트
        public async Task UpdateUserAsync(UserDb user)
        {
            var result = await _httpClient.PutAsJsonAsync($"api/gameserver/users/{user.DbId}", user);
            result.EnsureSuccessStatusCode();
        }

        // 인벤토리 아이템 업데이트
        public async Task UpdateInventoryItemAsync(InventoryDb item)
        {
            var result = await _httpClient.PutAsJsonAsync($"api/gameserver/inventory/{item.InventoryDbId}", item);
            result.EnsureSuccessStatusCode();
        }

        // 인벤토리 아이템 추가
        public async Task AddItemAsync(InventoryDb newItem)
        {
            var result = await _httpClient.PostAsJsonAsync("api/gameserver/additem", newItem);
            result.EnsureSuccessStatusCode();
        }

        public async Task<List<InventoryDb>> GetInventoryAsync(string userId)
        {
            return await _httpClient.GetFromJsonAsync<List<InventoryDb>>($"api/gameserver/users/{userId}/inventory");
        }
        #endregion GameServer DB Read
    }
}
