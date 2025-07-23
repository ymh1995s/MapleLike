using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AccountServer.DB
{
    [Table("Users")]
    public class AccountDB
    {
        [Key]
        public string ID { get; set; }

        [Required] // IS NOT NULL
        public string PW { get; set; }

        [Required]
        public string Salt { get; set; }

        public DateTime Created { get; set; }

        public DateTime? LastLogin { get; set; }
    }
}
