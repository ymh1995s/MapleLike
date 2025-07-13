using AccountServer.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AccountServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserTableController : ControllerBase
    {
        // Dependency Injection
        AppDbContext _context;

        public UserTableController(AppDbContext context)
        {
            _context = context;
        }

        // Create
        [HttpPost]
        public UserDb AddUser([FromBody] UserDb gameResult)
        {
            try
            {
                _context.Users.Add(gameResult);
                _context.SaveChanges();
                return gameResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
                return null;
            }
        }


        // Read
        [HttpGet]
        public List<UserDb> GetUsers()
        {
            List<UserDb> results = _context.Users
                .OrderByDescending(user => user.ID)
                .ToList();

            return results;
        }

        [HttpGet("{id}")]
        public UserDb GetUser(string id)
        {
            UserDb result = _context.Users
                        .Where(user => user.ID == id)
                        .FirstOrDefault();

            return result;
        }


        // Update
        [HttpPut]
        public bool UpdateUser([FromBody] UserDb gameResult)
        {
            var findResult = _context.Users
                .Where(x => x.ID == gameResult.ID)
                .FirstOrDefault();

            if (findResult == null)
                return false;

            // 아래 2개 외에 기획상 바꿀일 없음
            findResult.ID = gameResult.ID;
            findResult.LastLogin = gameResult.LastLogin;

            _context.SaveChanges();

            return true;
        }

        // Delete
        // 사용 계획 없음
        [HttpDelete("{id}")]
        public bool DeleteUser(string id)
        {
            var findResult = _context.Users
                        .Where(x => x.ID == id)
                        .FirstOrDefault();

            if (findResult == null)
                return false;

            _context.Users.Remove(findResult);
            _context.SaveChanges();

            return true;
        }
    }
}
