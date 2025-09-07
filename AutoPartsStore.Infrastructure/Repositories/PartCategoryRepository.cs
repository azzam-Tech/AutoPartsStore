using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.PartCategory;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsStore.Infrastructure.Repositories
{
    public class PartCategoryRepository : BaseRepository<PartCategory>, IPartCategoryRepository
    {
        public PartCategoryRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<PartCategoryDto>> GetAllCategoriesAsync()
        {
            return await _context.PartCategories
                .Where(pc => !pc.IsDeleted)
                .Select(pc => new PartCategoryDto
                {
                    Id = pc.Id,
                    CategoryName = pc.CategoryName,
                    ParentCategoryId = pc.ParentCategoryId,
                    ParentCategoryName = pc.ParentCategory != null ? pc.ParentCategory.CategoryName : null,
                    Description = pc.Description,
                    ImageUrl = pc.ImageUrl,
                    IsActive = pc.IsActive,
                    ProductsCount = pc.CarParts.Count(cp => !cp.IsDeleted),
                    SubCategories = pc.SubCategories
                        .Where(sc => !sc.IsDeleted && sc.IsActive)
                        .Select(sc => new PartCategoryDto
                        {
                            Id = sc.Id,
                            CategoryName = sc.CategoryName,
                            ImageUrl = sc.ImageUrl,
                            ProductsCount = sc.CarParts.Count(cp => !cp.IsDeleted)
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<PartCategoryDto> GetCategoryByIdAsync(int id)
        {
            return await _context.PartCategories
                .Where(pc => pc.Id == id && !pc.IsDeleted)
                .Select(pc => new PartCategoryDto
                {
                    Id = pc.Id,
                    CategoryName = pc.CategoryName,
                    ParentCategoryId = pc.ParentCategoryId,
                    ParentCategoryName = pc.ParentCategory != null ? pc.ParentCategory.CategoryName : null,
                    Description = pc.Description,
                    ImageUrl = pc.ImageUrl,
                    IsActive = pc.IsActive,
                    ProductsCount = pc.CarParts.Count(cp => !cp.IsDeleted),
                    SubCategories = pc.SubCategories
                        .Where(sc => !sc.IsDeleted && sc.IsActive)
                        .Select(sc => new PartCategoryDto
                        {
                            Id = sc.Id,
                            CategoryName = sc.CategoryName,
                            ImageUrl = sc.ImageUrl,
                            ProductsCount = sc.CarParts.Count(cp => !cp.IsDeleted)
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CategoryExistsAsync(string categoryName, int? excludeId = null)
        {
            var query = _context.PartCategories
                .Where(pc => pc.CategoryName.ToLower() == categoryName.ToLower() && !pc.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(pc => pc.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> HasSubCategoriesAsync(int categoryId)
        {
            return await _context.PartCategories
                .AnyAsync(pc => pc.ParentCategoryId == categoryId && !pc.IsDeleted);
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _context.CarParts
                .AnyAsync(cp => cp.CategoryId == categoryId && !cp.IsDeleted);
        }

        public async Task<int> GetProductsCountAsync(int categoryId)
        {
            return await _context.CarParts
                .CountAsync(cp => cp.CategoryId == categoryId && !cp.IsDeleted);
        }
    }
}