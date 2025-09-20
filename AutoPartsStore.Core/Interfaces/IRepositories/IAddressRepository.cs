using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.Address;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IAddressRepository : IBaseRepository<Address>
    {
        Task<IEnumerable<AddressDto>> GetByUserIdAsync(int userId);
        Task<AddressDto?> GetByIdWithDetailsAsync(int id);
        Task<bool> UserHasAddressAsync(int userId, int districtId, string streetName, string streetNumber, int? excludeId = null);
    }
}