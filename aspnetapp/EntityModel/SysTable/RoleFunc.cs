using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityModel
{
    [Table("SYS_RoleFunc")]
    public class RoleFunc
    {
        [Key]
        public int RoleFuncId { get; set; }
        public int RoleId { get; set; }
        public int FunctionId { get; set; }
        public int PresetValue { get; set; }
        public int IsUse { get; set; }

    }
}
