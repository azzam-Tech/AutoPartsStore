using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.Promotion;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsStore.Infrastructure.Repositories
{
    public class PromotionRepository : BaseRepository<Promotion>, IPromotionRepository
    {
        public PromotionRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<PromotionDto>> GetAllWithDetailsAsync()
        {
            return await _context.Promotions
                .Where(p => !p.IsDeleted)
                .Select(p => new PromotionDto
                {
                    Id = p.Id,
                    PromotionName = p.PromotionName,
                    Description = p.Description,
                    DiscountType = p.DiscountType,
                    DiscountValue = p.DiscountValue,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    IsActive = p.IsActive,
                    MinOrderAmount = p.MinOrderAmount,
                    IsDeleted = p.IsDeleted,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    IsActiveNow = p.IsActiveNow(),
                    ProductCount = p.ProductPromotions.Count
                })
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<PromotionDto> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Promotions
                .Where(p => p.Id == id && !p.IsDeleted)
                .Select(p => new PromotionDto
                {
                    Id = p.Id,
                    PromotionName = p.PromotionName,
                    Description = p.Description,
                    DiscountType = p.DiscountType,
                    DiscountValue = p.DiscountValue,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    IsActive = p.IsActive,
                    MinOrderAmount = p.MinOrderAmount,
                    IsDeleted = p.IsDeleted,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    IsActiveNow = p.IsActiveNow(),
                    ProductCount = p.ProductPromotions.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PromotionDto>> GetActivePromotionsAsync()
        {
            return await _context.Promotions
                .Where(p => p.IsActive && !p.IsDeleted && p.IsActiveNow())
                .Select(p => new PromotionDto
                {
                    Id = p.Id,
                    PromotionName = p.PromotionName,
                    Description = p.Description,
                    DiscountType = p.DiscountType,
                    DiscountValue = p.DiscountValue,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    IsActive = p.IsActive,
                    MinOrderAmount = p.MinOrderAmount,
                    ProductCount = p.ProductPromotions.Count
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductPromotionDto>> GetPromotionProductsAsync(int promotionId)
        {
            return await _context.ProductPromotions
                .Where(pp => pp.PromotionId == promotionId)
                .Include(pp => pp.CarPart)
                .Select(pp => new ProductPromotionDto
                {
                    Id = pp.Id,
                    PromotionId = pp.PromotionId,
                    PartId = pp.PartId,
                    PartName = pp.CarPart.PartName,
                    PartNumber = pp.CarPart.PartNumber,
                    CreatedAt = pp.CreatedAt,
                    UpdatedAt = pp.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> PromotionHasProductAsync(int promotionId, int partId)
        {
            return await _context.ProductPromotions
                .AnyAsync(pp => pp.PromotionId == promotionId && pp.PartId == partId);
        }
    }
}