using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityModel
{
    /// <summary>
    /// ��Ƶ��
    /// </summary>
    [Table("Video")]
    public class Video
    {
        [Key]
        public int Id { get; set; }

        public int? ClockId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Describe { get; set; } = string.Empty;

        /// <summary>
        /// �ϴ���id
        /// </summary>
        public int UploaderId { get; set; } 

        public string FileId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int IsDelete { get; set; }
    }
}