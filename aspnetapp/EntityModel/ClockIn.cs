using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace entityModel
{
    /// <summary>
    /// �ճ�ǩ����
    /// </summary>
    [Table("ClockIn")]
    public class ClockIn
    {
        /// <summary>
        /// ��Id
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// ΢�ŵ�openid
        /// </summary>
        public string OpenId { get; set; } = string.Empty;
        /// <summary>
        /// �����ҽ��id
        /// </summary>
        public string  DoctorId { get; set; }
        /// <summary>
        /// �������
        /// </summary>
        public string FeedbackComments { get; set; } = string.Empty;

        /// <summary>
        /// ��ѧ��Ƶ
        /// </summary>
        public int TeachVideoId { get; set; }
        /// <summary>
        /// �ճ���Ƶ
        /// </summary>
        public int DailyVideoId { get; set; }
        /// <summary>
        /// ǩ������
        /// </summary>
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}