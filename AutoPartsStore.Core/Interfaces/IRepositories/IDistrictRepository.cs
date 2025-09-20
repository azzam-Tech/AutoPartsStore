using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.District;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IDistrictRepository : IBaseRepository<District>
    {
        Task<IEnumerable<DistrictDto>> GetAllAsync();
        Task<IEnumerable<DistrictDto>> GetByCityIdAsync(int cityId);
        Task<DistrictDto> GetByIdWithDetailsAsync(int id);
        Task<bool> DistrictExistsAsync(int cityId, string districtName, int? excludeId = null);
        Task<int> GetAddressesCountAsync(int districtId);
    }
}