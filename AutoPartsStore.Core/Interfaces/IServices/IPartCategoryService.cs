using AutoPartsStore.Core.Models.PartCategory;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IPartCategoryService
    {
        Task<IEnumerable<PartCategoryDto>> GetAllCategoriesAsync();
        Task<PartCategoryDto> GetCategoryByIdAsync(int id);
        Task<PartCategoryDto> CreateCategoryAsync(CreatePartCategoryRequest request);
        Task<PartCategoryDto> UpdateCategoryAsync(int id, UpdatePartCategoryRequest request);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> ToggleCategoryStatusAsync(int id);
    }
}