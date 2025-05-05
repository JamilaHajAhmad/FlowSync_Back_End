using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationFlowSync.Models
{
    public class UserSession
    {
        public int Id { get; set; }
        // العلاقة مع المستخدم
        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public AppUser User { get; set; } = null!;

        public string DeviceInfo { get; set; } = string.Empty;// مثل المتصفح أو نظام التشغيل
        public string IPAddress { get; set; } = string.Empty;
        public string Token { get; set; } // اختياري إن كنت تحفظ JWT
        public DateTime LoginTime { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }
}
