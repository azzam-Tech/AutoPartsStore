using AutoPartsStore.Core.Models.District;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IDistrictService
    {
        Task<IEnumerable<DistrictDto>> GetAllAsync();
        Task<IEnumerable<DistrictDto>> GetByCityIdAsync(int cityId);
        Task<DistrictDto> GetByIdAsync(int id);
        Task<DistrictDto> CreateAsync(CreateDistrictRequest request);
        Task<DistrictDto> UpdateAsync(int id, UpdateDistrictRequest request);
        Task<bool> DeleteAsync(int id);
    }
}