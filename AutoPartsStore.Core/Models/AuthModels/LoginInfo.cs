
namespace AutoPartsStore.Core.Models.AuthModels
{
    public class LoginInfo
    {
        public bool login { get; set; }
        public string? accessToken { get; set; }
        public DateTime? expiresAt { get; set; }
        public UserInfoDto? userInfo { get; set; }
    }
}
