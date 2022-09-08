using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace entityModel
{
    [Table("We_MessageTemplate")]
    public class WeMessageTemplate
    {
        [Key]
        public long Id { get; set; }

        public string OpenId { get; set; } = string.Empty;
        /// <summary>
        /// 患者id
        /// </summary>
        public int PId { get; set; }
        public string TempId { get; set; } = string.Empty;
        public string TempName { get; set; } = string.Empty;
        public bool IS_Send { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

    }
}
