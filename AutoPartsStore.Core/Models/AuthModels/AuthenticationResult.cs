
namespace AutoPartsStore.Core.Models.AuthModels
{
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string AccessToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public UserInfoDto UserInfo { get; set; }

        public static AuthenticationResult SuccessResult(string message = null, string accessToken = null, DateTime? expiresAt = null, UserInfoDto userInfo = null)
        {
            return new AuthenticationResult
            {
                Success = true,
                Message = message,
                AccessToken = accessToken,
                ExpiresAt = expiresAt,
                UserInfo = userInfo
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
