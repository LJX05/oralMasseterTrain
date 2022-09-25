using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityModel
{
    
/// <summary>
    /// 患者表
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
        /// 出生日期 
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// 电话 
        /// </summary>
        public string Telephone { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        /// <summary>
        /// 最后一次签到时间
        /// </summary>
        public DateTime? LastCheckInTime { get; set; }

        public int? LastCheckInId { get; set; }
        /// <summary>
        /// 最后一次签到记录
        /// </summary>
        public virtual ClockIn LastCheckIn { get; set; } 
        /// <summary>
        /// 教学表
        /// </summary>
        public virtual IList<PatientToTeachVideo> PToVList{ get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
    /// <summary>
    /// 患者教学视频记录表
    /// </summary>
    [Table("PatientToTeachVideo")]
    public class PatientToTeachVideo
    {
        [Key]
        public int Id { get; set; }

        public string OpenId { get; set; } = string.Empty;
        /// <summary>
        /// 患者id
        /// </summary>
        public int? PId { get; set; }

        /// <summary>
        /// 教学视频id
        /// </summary>
        public int? TVId { get; set; }
        /// <summary>
        /// 教学视频
        /// </summary>
        public virtual Video TVideo { get; set; } 
    }


    /// <summary>
    /// 患者活动记录表
    /// </summary>
    [Table("PatientActivity")]
    public class PatientActivity
    {
        [Key]
        public int Id { get; set; }

        public string OpenId { get; set; } = string.Empty;
        /// <summary>
        /// 患者id
        /// </summary>
        public int PId { get; set; } 
        /// <summary>
        /// 视频id
        /// </summary>
        public string VideoIds { get; set; } = string.Empty;
        
        /// <summary>
        /// 记录内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}