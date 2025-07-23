using Google.Protobuf.Protocol;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerContents.DB;

namespace AccountServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameServerController : ControllerBase
    {
        // Dependency Injection
        // ASP.NET의 의존성 주입 컨테이너가 자동으로 수명관리를 해주어 using 불필요
        private readonly GameDbContext _context;

        public GameServerController(GameDbContext context)
        {
            _context = context;
        }

        #region 유저 정보 조회/수정

        // 전체 유저 조회
        [HttpGet("Users")]
        public async Task<ActionResult<List<UserDb>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // 특정 유저 조회
        [HttpGet("Users/{dbId}")]
        public async Task<ActionResult<UserDb>> GetUser(string dbId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.DbId == dbId);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // 유저 정보 수정
        [HttpPut("Users/{dbId}")]
        public async Task<IActionResult> UpdateUser(string dbId, [FromBody] UserDb updatedUser)
        {
            if (dbId != updatedUser.DbId)
                return BadRequest("ID 불일치");

            var user = await _context.Users.FindAsync(dbId);
            if (user == null)
                return NotFound();

            // 수정할 내용
            user.Gold = updatedUser.Gold;
            user.STR = updatedUser.STR;
            user.DEX = updatedUser.DEX;
            user.INT = updatedUser.INT;
            user.LUK = updatedUser.LUK;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion 유저 정보 조회/수정

        #region 인벤토리 정보 조회/수정

        // 특정 유저 인벤토리 조회
        [HttpGet("Users/{dbId}/Inventory")]
        public async Task<ActionResult<List<InventoryDb>>> GetUserInventory(string dbId)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Item)
                .Where(i => i.UserDbId == dbId)
                .ToListAsync();

            return Ok(inventory);
        }

        // 인벤토리 아이템 수정
        [HttpPut("Inventory/{inventoryId}")]
        public async Task<IActionResult> UpdateInventoryItem(int inventoryId, [FromBody] InventoryDb updatedItem)
        {
            if (inventoryId != updatedItem.InventoryDbId)
                return BadRequest("ID 불일치");

            var item = await _context.Inventories.FindAsync(inventoryId);
            if (item == null)
                return NotFound();

            // 수정할 내용
            item.Count = updatedItem.Count;
            item.IsEquipped = updatedItem.IsEquipped;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // 인벤토리 아이템 추가
        public async Task<IActionResult> AddItemToInventory(string userDbId, ItemType itemType, int amount = 1)
        {
            // 1. 유저 + 인벤토리 목록 로딩
            var user = await _context.Users
                .Include(u => u.Inventory)
                .FirstOrDefaultAsync(u => u.DbId == userDbId);

            if (user == null)
                return NotFound("User not found");

            // 2. 인벤토리에서 해당 아이템 존재 여부 확인
            InventoryDb inventoryItem = await _context.Inventories
                .FirstOrDefaultAsync(i => i.UserDbId == userDbId && i.ItemDbId == itemType);

            if (inventoryItem != null)
            {
                // 3. 이미 존재하는데 소비아이템이면 수량만 추가
                if((int)inventoryItem.ItemDbId < 1000)
                {
                    inventoryItem.Count += amount;
                }
                // 4. 이미 존재하는데 장비아이템이면 새로 추가
                else
                {
                    AddNewInventoryItem(userDbId, itemType, amount);
                }
            }
            else
            {
                // 5. 존재하지 않으면 새로 추가
                AddNewInventoryItem(userDbId, itemType, amount);
            }

            // 3. 저장
            await _context.SaveChangesAsync();
            return Ok("아이템 추가 완료");
        }
        private void AddNewInventoryItem(string userDbId, ItemType itemType, int amount)
        {
            var newItem = new InventoryDb
            {
                ItemDbId = itemType,
                UserDbId = userDbId,
                Count = amount,
                MaxCount = 999
            };

            _context.Inventories.Add(newItem);
        }

        #endregion 인벤토리 정보 조회/수정
    }
}
