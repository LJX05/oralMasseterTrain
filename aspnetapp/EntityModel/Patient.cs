using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace entityModel
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

        public string DoctorId { get; set; } = string.Empty;
        /// <summary>
        /// 最后一次签到时间
        /// </summary>
        public DateTime? LastCheckInTime { get; set; }
        public int? LastCheckInVideoId { get; set; }
        /// <summary>
        ///当前视频Id
        /// </summary>
        public int? TeachVideoId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
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
        public int VideoId { get; set; } 
        /// <summary>
        /// 记录内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}