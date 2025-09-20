using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.CarPart;

namespace AutoPartsStore.Core.Interfaces
{
    public interface ICarPartRepository : IBaseRepository<CarPart>
    {
        Task<CarPartDto> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<CarPartDto>> GetFilteredAsync(CarPartFilter filter);
        Task<IEnumerable<CarPartDto>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<CarPartDto>> GetFeaturedAsync(int count);
        Task<bool> PartNumberExistsAsync(string partNumber, int? excludeId = null);
        Task<int> GetTotalCountAsync(CarPartFilter filter);
        Task<decimal> GetMaxPriceAsync();
        Task<List<CarPart>> GetPartsByPromotionIdAsync(int promotionId);

    }
}