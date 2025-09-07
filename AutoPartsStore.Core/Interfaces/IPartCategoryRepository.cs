using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.PartCategory;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IPartCategoryRepository : IBaseRepository<PartCategory>
    {
        Task<IEnumerable<PartCategoryDto>> GetAllCategoriesAsync();
        Task<PartCategoryDto> GetCategoryByIdAsync(int id);
        Task<bool> CategoryExistsAsync(string categoryName, int? excludeId = null);
        Task<bool> HasSubCategoriesAsync(int categoryId);
        Task<bool> HasProductsAsync(int categoryId);
        Task<int> GetProductsCountAsync(int categoryId);
    }
}