using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.City;

namespace AutoPartsStore.Core.Interfaces
{
    public interface ICityRepository : IBaseRepository<City>
    {
        Task<IEnumerable<CityDto>> GetAllAsync();
        Task<CityDto> GetByIdWithDistrictsAsync(int id);
        Task<bool> CityNameExistsAsync(string cityName, int? excludeId = null);
        Task<int> GetDistrictsCountAsync(int cityId);
    }
}