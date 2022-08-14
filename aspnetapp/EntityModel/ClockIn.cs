using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace entityModel
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
        public int OpenId { get; set; }
        /// <summary>
        /// 负责的医生id
        /// </summary>
        public int  DoctorId { get; set; }
        /// <summary>
        /// 反馈意见
        /// </summary>
        public string FeedbackComments { get; set; } = string.Empty;

        /// <summary>
        /// 教学视频
        /// </summary>
        public int TeachVideoId { get; set; }
        /// <summary>
        /// 日常视频
        /// </summary>
        public int DailyVideoId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}