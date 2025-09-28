using AutoPartsStore.Core.Models;
using AutoPartsStore.Core.Models.CarPart;

namespace AutoPartsStore.Core.Interfaces
{
    public interface ICarPartService
    {
        Task<CarPartDto> GetByIdAsync(int id);
        Task<PagedResult<CarPartDto>> GetFilteredAsync(CarPartFilter filter);
        Task<IEnumerable<CarPartDto>> GetFeaturedAsync(int count = 8);
        Task<CarPartDto> CreateAsync(CreateCarPartRequest request);
        Task<CarPartDto> UpdateAsync(int id, UpdateCarPartRequest request);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateStockAsync(int id, int quantity);
        Task<bool> UpdatePriceAsync(int id, decimal price);
        Task<decimal> GetMaxPriceAsync();
        Task<bool> DeactivateAsync(int id);
        Task<bool> ActivateAsync(int id);


    }
}