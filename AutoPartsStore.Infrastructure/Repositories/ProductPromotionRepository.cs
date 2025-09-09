using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.Promotion;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsStore.Infrastructure.Repositories
{
    public class ProductPromotionRepository : BaseRepository<ProductPromotion>, IProductPromotionRepository
    {
        public ProductPromotionRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<ProductPromotionDto>> GetByPromotionIdAsync(int promotionId)
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

        public async Task<IEnumerable<ProductPromotionDto>> GetByPartIdAsync(int partId)
        {
            return await _context.ProductPromotions
                .Where(pp => pp.PartId == partId)
                .Include(pp => pp.Promotion)
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

        public async Task<bool> ProductHasActivePromotionAsync(int partId)
        {
            return await _context.ProductPromotions
                .Include(pp => pp.Promotion)
                .AnyAsync(pp => pp.PartId == partId &&
                               pp.Promotion.IsActive &&
                               !pp.Promotion.IsDeleted &&
                               pp.Promotion.IsActiveNow());
        }
    }
}