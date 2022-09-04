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

        public string OpenId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Sex { get; set; } = string.Empty;

        public string DoctorId { get; set; } = string.Empty;
        /// <summary>
        /// ���һ��ǩ��ʱ��
        /// </summary>
        public DateTime? LastCheckInTime { get; set; }
        public int? LastCheckInVideoId { get; set; }
        /// <summary>
        ///��ǰ��ƵId
        /// </summary>
        public int? TeachVideoId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }


    /// <summary>
    /// ���߻��¼��
    /// </summary>
    [Table("PatientActivity")]
    public class PatientActivity
    {
        [Key]
        public int Id { get; set; }

        public string OpenId { get; set; } = string.Empty;
        /// <summary>
        /// ����id
        /// </summary>
        public int PId { get; set; } 
        /// <summary>
        /// ��Ƶid
        /// </summary>
        public int VideoId { get; set; } 
        /// <summary>
        /// ��¼����
        /// </summary>
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}