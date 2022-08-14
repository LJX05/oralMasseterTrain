using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace entityModel
{
    [Table("SYS_Function")]
    public class Function
    {
        [Key]
        public int FunctionId { get; set; }
        public int ParentId { get; set; }
        public int SetRank { get; set; }
        public int FunctionType { get; set; }
        public string FunctionTag { get; set; } = string.Empty;
        public string FunctionIco { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleUrl { get; set; } = string.Empty;
        public string Mark { get; set; } = string.Empty;
        public int Sort { get; set; }
        public string _RS_ { get; set; } = string.Empty;
    }
}
