namespace AutoPartsStore.Core.Models.AuthModels.EmailAuthModels
{
    public class CompleteRegistrationRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
