using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.PartCategory;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace AutoPartsStore.Infrastructure.Services
{
    public class PartCategoryService : IPartCategoryService
    {
        private readonly IPartCategoryRepository _categoryRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<PartCategoryService> _logger;

        public PartCategoryService(IPartCategoryRepository categoryRepository, AppDbContext context, ILogger<PartCategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<PartCategoryDto>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllCategoriesAsync();
        }

        public async Task<PartCategoryDto> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.GetCategoryByIdAsync(id);
        }

        public async Task<PartCategoryDto> CreateCategoryAsync(CreatePartCategoryRequest request)
        {
            if (await _categoryRepository.CategoryExistsAsync(request.CategoryName))
                throw new InvalidOperationException($"Category '{request.CategoryName}' already exists.");

            if (request.ParentCategoryId.HasValue &&
                await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value) == null)
                throw new InvalidOperationException("Parent category not found.");

            var category = new PartCategory(request.CategoryName, request.ParentCategoryId,
                                          request.Description, request.ImageUrl);
            category.Activate();

            _context.PartCategories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category created: {CategoryName}", request.CategoryName);
            return await _categoryRepository.GetCategoryByIdAsync(category.Id);
        }

        public async Task<PartCategoryDto> UpdateCategoryAsync(int id, UpdatePartCategoryRequest request)
        {
            var category = await _context.PartCategories.FindAsync(id);
            if (category == null || category.IsDeleted)
                throw new KeyNotFoundException("Category not found.");

            if (await _categoryRepository.CategoryExistsAsync(request.CategoryName, id))
                throw new InvalidOperationException($"Category '{request.CategoryName}' already exists.");

            category.Update(request.CategoryName, request.Description, request.ImageUrl, request.ParentCategoryId);
            if (request.IsActive)
            {
                category.Activate();
            }
            else { category.Deactivate(); }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Category updated: {CategoryName}", request.CategoryName);
            return await _categoryRepository.GetCategoryByIdAsync(category.Id);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.PartCategories.FindAsync(id);
            if (category == null || category.IsDeleted)
                throw new KeyNotFoundException("Category not found.");

            if (await _categoryRepository.HasSubCategoriesAsync(id))
                throw new InvalidOperationException("Cannot delete category with subcategories.");

            if (await _categoryRepository.HasProductsAsync(id))
                throw new InvalidOperationException("Cannot delete category with products.");

            category.SoftDelete();
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category deleted: {CategoryName}", category.CategoryName);
            return true;
        }

        public async Task<bool> ToggleCategoryStatusAsync(int id)
        {
            var category = await _context.PartCategories.FindAsync(id);
            if (category == null || category.IsDeleted)
                throw new KeyNotFoundException("Category not found.");

            if (category.IsActive)
                category.Deactivate();
            else
                category.Activate();

            await _context.SaveChangesAsync();

            _logger.LogInformation("Category {Action}: {CategoryName}",
                category.IsActive ? "activated" : "deactivated", category.CategoryName);
            return category.IsActive;
        }
    }
}