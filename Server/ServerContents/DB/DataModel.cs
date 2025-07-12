using System.ComponentModel.DataAnnotations.Schema;

namespace ServerContents.DB
{
    [Table("Account")]
    public class AccountDb
    {
        public int AccountDbId { get; set; } //클래스명 + Id 하면 자동으로 주키로 인식
        public string AccountName { get; set; }
        public ICollection<PlayerDb> Players { get; set; }
    }

    [Table("Player")]
    public class PlayerDb
    {
        public int PlayerDbId { get; set; } //클래스명 + Id 하면 자동으로 주키로 인식
        public string PlayerName { get; set; }
        public AccountDb Account { get; set; }
    }
}
