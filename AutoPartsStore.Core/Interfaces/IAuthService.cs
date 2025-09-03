using System.Threading.Tasks;

namespace AutoPartsStore.Core.Interfaces
{
    /// <summary>
    /// واجهة خدمة المصادقة (Authentication)
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// التحقق من صحة بيانات الدخول (اسم مستخدم وكلمة مرور)
        /// </summary>
        Task<bool> ValidateCredentialsAsync(string username, string password);

        /// <summary>
        /// إنشاء JWT Token للمستخدم
        /// </summary>
        Task<AuthenticationResult> GenerateJwtTokenAsync(string username);

        /// <summary>
        /// تسجيل مستخدم جديد
        /// </summary>
        Task<AuthenticationResult> RegisterAsync(
            string username,
            string email,
            string fullName,
            string phoneNumber,
            string password);
    }

    /// <summary>
    /// نتيجة عملية المصادقة (نجاح/فشل، توكن، رسالة)
    /// </summary>
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? AccessToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}