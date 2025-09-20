using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models;
using AutoPartsStore.Core.Models.Promotions;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsStore.Infrastructure.Repositories
{
    public class PromotionRepository : BaseRepository<Promotion>, IPromotionRepository
    {
        public PromotionRepository(AppDbContext context) : base(context) { }

        public async Task<PagedResult<PromotionDto>> GetAllWithDetailsAsync(PromotionFilter filter)
        {
            var query = _context.Promotions
                .Where(p => !p.IsDeleted).AsQueryable();

            if (filter.isActive.HasValue)
                query = query.Where(p => p.IsActive == filter.isActive);
            if (filter.discountType.HasValue)
                query =query.Where(p => p.DiscountType == filter.discountType);

            var totalCount = _context.Promotions.Count(p => !p.IsDeleted); ;
            if (filter.isActive.HasValue)
                totalCount = _context.Promotions.Count(p => !p.IsDeleted && p.IsActive == filter.isActive);
            var totalPages = (int)Math.Ceiling(totalCount / (float)filter.pageSize);

            // Apply pagination
            query = query.Skip((filter.pageNum - 1) * filter.pageSize)
           .Take(filter.pageSize);

            var pagedResult = new PagedResult<PromotionDto>
            {
                CurrentPage = filter.pageNum,
                PageSize = filter.pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = await query.Select(p => new PromotionDto
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
                    ProductCount = p.CarParts != null ? p.CarParts.Count : 0
                })
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync()
            };
            return pagedResult;
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
                    ProductCount = p.CarParts.Count
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
                    ProductCount = p.CarParts != null ? p.CarParts.Count : 0
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<PartPromotionDto>> GetPromotionProductsAsync(int promotionId)
        {
            return await _context.CarParts
                .Where(cp => cp.PromotionId == promotionId)
                .Select(pp => new PartPromotionDto
                {
                    PartId = pp.Id,
                    PromotionId = pp.PromotionId!.Value,
                    PartName = pp.PartName,
                    PartNumber = pp.PartNumber,
                    CreatedAt = pp.CreatedAt,
                    UpdatedAt = pp.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> PromotionHasProductAsync(int promotionId, int partId)
        {
            return await _context.CarParts
                .AnyAsync(cp => cp.PromotionId == promotionId && cp.Id == partId);
        }

        public async Task<List<Promotion>> GetExpiredActivePromotionsAsync()
        {
            return await _context.Promotions
                .Where(p => p.IsActive && p.EndDate < DateTime.UtcNow)
                .ToListAsync();
        }
    }
}