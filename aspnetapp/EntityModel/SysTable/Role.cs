using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityModel
{
    [Table("SYS_Role")]
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; }=string.Empty;
        public int IsPreset { get; set; }
        public string RoleMark { get; set; } = string.Empty;
        public int Creator { get; set; }
        public string _RS_ { get; set; } = string.Empty;
        public string Groups { get; set; } = string.Empty;
    }
}
