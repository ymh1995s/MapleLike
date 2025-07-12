using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ServerContents.DB
{
    // 데이터베이스 컨텍스트 클래스, DbContext를 상속받아 데이터베이스와의 상호작용을 정의
    public class AppDbContext : DbContext
    {
        // Accounts 테이블과 매핑되는 DbSet
        public DbSet<AccountDb> Accounts { get; set; }
        // Players 테이블과 매핑되는 DbSet
        public DbSet<PlayerDb> Players { get; set; }

        // 로깅 설정을 위한 ILoggerFactory 인스턴스, 콘솔 출력
        static readonly ILoggerFactory _logger = LoggerFactory.Create(builder => { builder.AddConsole(); });

        // DbContext의 동작 설정
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options
                .UseLoggerFactory(_logger)
                .UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=GameServerDb;Integrated Security=True;");
        }

        // 데이터 모델 생성 시 추가적인 설정
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // AccountDb 엔터티에 대해 AccountName 속성을 유니크로 설정
            builder.Entity<AccountDb>()
                .HasIndex(a => a.AccountName) // AccountName에 인덱스 추가
                .IsUnique();

            // PlayerDb 엔터티에 대해 PlayerName 속성을 유니크로 설정
            builder.Entity<PlayerDb>()
                .HasIndex(p => p.PlayerName) // PlayerName에 인덱스 추가
                .IsUnique();
        }
    }
}
