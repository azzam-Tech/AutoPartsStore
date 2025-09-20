
namespace AutoPartsStore.Core.Models.AuthModels
{
    public class UserInfoDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public List<string> Permissions { get; set; } = new List<string>();
        public string RedirectTo { get; set; }
        public SessionInfoDto SessionInfo { get; set; }
    }
}
