using System.Threading.Tasks;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IAuthService
    {
        Task<bool> ValidateCredentialsAsync(string username, string password);
        Task<AuthenticationResult> GenerateJwtTokenAsync(string username);
        Task<AuthenticationResult> RegisterAsync(
            string username,
            string email,
            string fullName,
            string phoneNumber,
            string password);
    }

    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string AccessToken { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public static AuthenticationResult SuccessResult(string message = null, string accessToken = null, DateTime? expiresAt = null)
        {
            return new AuthenticationResult
            {
                Success = true,
                Message = message,
                AccessToken = accessToken,
                ExpiresAt = expiresAt
            };
        }

        public static AuthenticationResult FailureResult(string message)
        {
            return new AuthenticationResult
            {
                Success = false,
                Message = message
            };
        }
    }
}