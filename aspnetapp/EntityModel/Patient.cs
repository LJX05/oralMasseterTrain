using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace entityModel
{
    /// <summary>
    /// ���߱�
    /// </summary>
    [Table("Patient")]
    public class Patient
    {
        [Key]
        public int Id { get; set; }

        public int OpenId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Sex { get; set; } = string.Empty;

        public int DoctorId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}