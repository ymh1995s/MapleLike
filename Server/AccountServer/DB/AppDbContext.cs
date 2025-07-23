using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AccountServer.DB
{
    // 데이터베이스 컨텍스트 클래스, DbContext를 상속받아 데이터베이스와의 상호작용을 정의
    public class AppDbContext : DbContext
    {
        public DbSet<AccountDB> Users { get; set; }

        // 로깅 설정을 위한 ILoggerFactory 인스턴스, 콘솔 출력
        static readonly ILoggerFactory _logger = LoggerFactory.Create(builder => { builder.AddConsole(); });

        // 의존성 주입을 위해 깡통 생성자 필요 : DbContext 옵션을 전달받기 위한 생성자
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        // DbContext의 동작 설정
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
        }

        // 데이터 모델 생성 시 추가적인 설정
        protected override void OnModelCreating(ModelBuilder builder)
        {

        }
    }
}
