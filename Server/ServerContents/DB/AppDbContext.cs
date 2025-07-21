using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ServerContents.DB
{
    // 데이터베이스 컨텍스트 클래스, DbContext를 상속받아 데이터베이스와의 상호작용을 정의
    public class AppDbContext : DbContext
    {
        public DbSet<UserDb> Users { get; set; }
        public DbSet<ItemDb> Items { get; set; }
        public DbSet<ItemEquipDb> ItemEquips { get; set; }
        public DbSet<ItemConsumableDb> ItemConsumables { get; set; }
        public DbSet<InventoryDb> Inventories { get; set; }

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
            builder.Entity<ItemDb>()
            .HasIndex(i => i.Name) // 닉네임만 유니크하도록 설정
            .IsUnique();

            // InventoryDb.UserDbId에 인덱싱 추가
            // UserDbId를 통해 인벤토리를 탐색할 때 성능 향상
            builder.Entity<InventoryDb>()
                .HasIndex(inv => inv.UserDbId);

            // InventoryDb: ItemDbId 인덱스 (필터나 장비 장착 시)
            // InventoryDb에서 ItemDbId로 찾을 때 성능 향상.
            // 현재는 효과가 미미하지만 볼륨이 커지면 성능 향상 기대
            builder.Entity<InventoryDb>()
                .HasIndex(inv => inv.ItemDbId);
        }
    }
}
