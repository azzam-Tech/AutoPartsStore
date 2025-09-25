using AutoPartsStore.Core.Models.AuthModels;
using AutoPartsStore.Core.Models.AuthModels.EmailAuthModels;
using System.Threading.Tasks;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IAuthService
    {
        //Task<AuthenticationResult> GenerateJwtTokenAsync(string username);
        //Task<AuthenticationResult> RegisterAsync(
        //    string username,
        //    string email,
        //    string fullName,
        //    string phoneNumber,
        //    string password);
        Task<bool> SendCode(SendCodeRequest request);
        Task<AuthenticationResult> VerifyCode(VerifyCodeRequest request);
        Task<AuthenticationResult> CompleteRegistration(CompleteRegistrationRequest request);

    }


}