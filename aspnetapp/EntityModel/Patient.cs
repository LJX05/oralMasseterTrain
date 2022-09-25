using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityModel
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

        /// <summary>
        /// �������� 
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// �绰 
        /// </summary>
        public string Telephone { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        /// <summary>
        /// ���һ��ǩ��ʱ��
        /// </summary>
        public DateTime? LastCheckInTime { get; set; }

        public int? LastCheckInId { get; set; }
        /// <summary>
        /// ���һ��ǩ����¼
        /// </summary>
        public virtual ClockIn LastCheckIn { get; set; } 
        /// <summary>
        /// ��ѧ��
        /// </summary>
        public virtual IList<PatientToTeachVideo> PToVList{ get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
    /// <summary>
    /// ���߽�ѧ��Ƶ��¼��
    /// </summary>
    [Table("PatientToTeachVideo")]
    public class PatientToTeachVideo
    {
        [Key]
        public int Id { get; set; }

        public string OpenId { get; set; } = string.Empty;
        /// <summary>
        /// ����id
        /// </summary>
        public int? PId { get; set; }

        /// <summary>
        /// ��ѧ��Ƶid
        /// </summary>
        public int? TVId { get; set; }
        /// <summary>
        /// ��ѧ��Ƶ
        /// </summary>
        public virtual Video TVideo { get; set; } 
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
        public string VideoIds { get; set; } = string.Empty;
        
        /// <summary>
        /// ��¼����
        /// </summary>
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}