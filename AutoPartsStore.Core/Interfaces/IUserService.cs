using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(int userId);
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<List<UserRole>> GetUserRolesAsync(int userId);
    }
}
