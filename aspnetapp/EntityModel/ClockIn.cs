using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityModel
{
    /// <summary>
    /// 日常签到表
    /// </summary>
    [Table("ClockIn")]
    public class ClockIn
    {
        /// <summary>
        /// 表Id
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 微信的openid
        /// </summary>
        public string OpenId { get; set; } = string.Empty;
        /// <summary>
        /// 负责的医生id
        /// </summary>
        public string  DoctorId { get; set; }
        /// <summary>
        /// 反馈意见
        /// </summary>
        public string FeedbackComments { get; set; } = string.Empty;

        /// <summary>
        /// 教学视频
        /// </summary>
        public string TeachVideoId { get; set; } = string.Empty;



        public virtual IList<Video> Videos { get; set; }

        /// <summary>
        /// 签到日期
        /// </summary>
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}