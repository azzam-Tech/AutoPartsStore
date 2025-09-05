using AutoPartsStore.Core.Models.AuthModels;
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

    
}