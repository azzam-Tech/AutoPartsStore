using AutoPartsStore.Core.Models.City;

namespace AutoPartsStore.Core.Interfaces
{
    public interface ICityService
    {
        Task<IEnumerable<CityDto>> GetAllAsync();
        Task<CityDto> GetByIdAsync(int id);
        Task<CityDto> CreateAsync(CreateCityRequest request);
        Task<CityDto> UpdateAsync(int id, UpdateCityRequest request);
        Task<bool> DeleteAsync(int id);
    }
}