using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace entityModel
{
    [Table("SYS_User")]
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Account { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public int Creator { get; set; }
        public int IsPreset { get; set; }
        public int IsLocked { get; set; }
        public int IsDeleted { get; set; }
        public string Token2 { get; set; } = string.Empty;
        public string IPLimits { get; set; } = string.Empty;
        public string DTLimits { get; set; } = string.Empty;
        public string _RS_ { get; set; } = string.Empty;
        public DateTime FirstTime { get; set; }
        public DateTime LastTime { get; set; }
        public string LastLoginIP { get; set; } = string.Empty;
        public int LoginNum { get; set; }
        public int OnlineTime { get; set; }
        public string Groups { get; set; } = string.Empty;
        public int _TS_ { get; set; }
        public string TrueName { get; set; } = string.Empty;
        public int IdentityNo { get; set; }
        public string IMEI { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public int OnlineFlag { get; set; }
    }
}
