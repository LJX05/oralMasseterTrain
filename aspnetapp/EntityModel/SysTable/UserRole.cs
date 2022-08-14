using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace entityModel
{
    [Table("SYS_UserRole")]
    public class UserRole
    {
        [Key]
        public int UserRoleId { get; set; }
        public int RoleId { get; set; }
        public int UserId { get; set; }
        public string _RS_ { get; set; } = string.Empty;
    }
}
