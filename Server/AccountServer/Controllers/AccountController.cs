using AccountServer.DB;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace AccountServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        // Dependency Injection
        AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // ASP.NET Core에서 제공하는 헬퍼 메서드
        //  Ok(object)  200 OK 성공 + JSON 등 객체 반환
        // Created(string uri, object) 201 Created 리소스 생성됨(등록 시 사용)
        // NoContent() 204 No Content  성공했지만 반환할 콘텐츠 없음
        // BadRequest(object)  400 Bad Request 요청이 잘못됐을 때
        // Unauthorized()  401 Unauthorized 인증 실패(로그인 실패 등)
        // Forbid()    403 Forbidden 권한 없음(인증은 됐지만 권한 없음)
        // NotFound(object)    404 Not Found   리소스 없음
        // Conflict(object)    409 Conflict 중복 등의 충돌 발생 시
        // StatusCode(int, object) 원하는 코드  직접 응답코드 지정 가능(예: 500)

        // Create
        [HttpPost]
        public IActionResult LoginOrRegister([FromBody] AccountDB input)
        {
            try
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.ID == input.ID);

                // 랜덤 난수(토큰) 생성
                string secretValue = GenerateToken();

                if (existingUser == null)
                {
                    // 새 아이디 생성
                    string salt = GenerateSalt();
                    string hashed = HashPassword(input.PW, salt);

                    var newUser = new AccountDB
                    {
                        ID = input.ID,
                        PW = hashed,
                        Salt = salt,
                        Created = DateTime.UtcNow,
                        LastLogin = DateTime.UtcNow
                    };

                    _context.Users.Add(newUser);
                    _context.SaveChanges();


                    return Ok(new 
                    {
                        Message = "RegisterSuccess", 
                        ID = newUser.ID,
                        SecretValue = secretValue
                    });
                }
                else
                {
                    // 이미 아이디가 있을 때 인증
                    bool pwMatch = VerifyPassword(input.PW, existingUser.PW, existingUser.Salt);

                    // 비밀번호 틀림 
                    if (!pwMatch)
                        return Unauthorized(new { Message = "Wrong password 이 힌트는 주지 않는게 좋음" });

                    existingUser.LastLogin = DateTime.UtcNow;
                    _context.SaveChanges();

                    return Ok(new 
                    { Message = 
                        "LoginSuccess", 
                        ID = existingUser.ID,
                        SecretValue = secretValue
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Server Error");
            }
        }


        // Read
        [HttpGet]
        public List<AccountDB> GetUsers()
        {
            List<AccountDB> results = _context.Users
                .OrderByDescending(user => user.ID)
                .ToList();

            return results;
        }

        [HttpGet("{id}")]
        public AccountDB GetUser(string id)
        {
            AccountDB result = _context.Users
                        .Where(user => user.ID == id)
                        .FirstOrDefault();

            return result;
        }

        // Update
        [HttpPut]
        public bool UpdateUser([FromBody] AccountDB gameResult)
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

        private const int SaltSize = 16; // 128비트
        private const int HashSize = 32; // 256비트 => SHA256을 쓰기 때문에 32비트로 맞춰줌
        private const int Iterations = 100_000;

        // 솔트 생성
        private string GenerateSalt()
        {
            byte[] saltBytes = new byte[SaltSize];
            // 암호 관련 RandomNumberGenerator클래스 => 네티이브 자원 사용 => using으로 회수
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        // 해시 생성
        // PBKDF2 알고리즘을 구현한 클래스가 Rfc2898DeriveBytes이다.
        // 그 안에서 제일 무난한 SHA256 해시 알고리즘 사용
        private string HashPassword(string password, string saltBase64)
        {
            byte[] saltBytes = Convert.FromBase64String(saltBase64);

            // 암호 관련 Rfc2898DeriveBytes => 네티이브 자원 사용 => using으로 회수
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hashBytes = pbkdf2.GetBytes(HashSize);
                return Convert.ToBase64String(hashBytes);
            }
        }

        // 비밀번호 검증
        private bool VerifyPassword(string inputPw, string storedHash, string saltBase64)
        {
            string inputHash = HashPassword(inputPw, saltBase64);
            return storedHash == inputHash;
        }

        private string GenerateToken()
        {
            byte[] tokenBytes = new byte[32]; // 256비트 토큰
            // RandomNumberGenerator : Cryptographic클래스의 난수 생성기(보안용 랜덤값)
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            return Convert.ToBase64String(tokenBytes);
        }
    }
}
