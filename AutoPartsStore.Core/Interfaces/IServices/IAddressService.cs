using AutoPartsStore.Core.Models.Address;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressDto>> GetUserAddressesAsync(int userId);
        Task<AddressDto?> GetByIdAsync(int id);
        Task<AddressDto> CreateAsync(CreateAddressRequest request);
        Task<AddressDto> UpdateAsync(int id, UpdateAddressRequest request);
        Task<bool> DeleteAsync(int id);
        Task<bool> SetDefaultAddressAsync(int userId, int addressId);
    }
}