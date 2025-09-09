using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.Promotion;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoPartsStore.Infrastructure.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IProductPromotionRepository _productPromotionRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<PromotionService> _logger;

        public PromotionService(
            IPromotionRepository promotionRepository,
            IProductPromotionRepository productPromotionRepository,
            AppDbContext context,
            ILogger<PromotionService> logger)
        {
            _promotionRepository = promotionRepository;
            _productPromotionRepository = productPromotionRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync()
        {
            return await _promotionRepository.GetAllWithDetailsAsync();
        }

        public async Task<PromotionDto> GetPromotionByIdAsync(int id)
        {
            return await _promotionRepository.GetByIdWithDetailsAsync(id);
        }

        public async Task<IEnumerable<PromotionDto>> GetActivePromotionsAsync()
        {
            return await _promotionRepository.GetActivePromotionsAsync();
        }

        public async Task<PromotionDto> CreatePromotionAsync(CreatePromotionRequest request)
        {
            var promotion = new Promotion(
                request.PromotionName,
                request.DiscountType,
                request.DiscountValue,
                request.StartDate,
                request.EndDate,
                request.MinOrderAmount,
                request.Description
            );

            await _context.Promotions.AddAsync(promotion);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Promotion created: {PromotionName}", request.PromotionName);
            return await _promotionRepository.GetByIdWithDetailsAsync(promotion.Id);
        }

        public async Task<PromotionDto> UpdatePromotionAsync(int id, UpdatePromotionRequest request)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null || promotion.IsDeleted)
                throw new KeyNotFoundException("Promotion not found.");

            // التحقق من صحة التواريخ إذا تم توفيرها
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate >= request.EndDate)
                throw new ArgumentException("Start date must be before end date");

            // التحديث باستخدام الأساليب
            if (!string.IsNullOrEmpty(request.PromotionName) || request.Description != null || request.MinOrderAmount.HasValue)
            {
                promotion.UpdateBasicInfo(
                    request.PromotionName ?? promotion.PromotionName,
                    request.Description ?? promotion.Description,
                    request.MinOrderAmount ?? promotion.MinOrderAmount
                );
            }

            if (request.DiscountType.HasValue || request.DiscountValue.HasValue)
            {
                promotion.UpdateDiscountInfo(
                    request.DiscountType ?? promotion.DiscountType,
                    request.DiscountValue ?? promotion.DiscountValue
                );
            }

            if (request.StartDate.HasValue || request.EndDate.HasValue)
            {
                promotion.UpdateDateRange(
                    request.StartDate ?? promotion.StartDate,
                    request.EndDate ?? promotion.EndDate
                );
            }

            if (request.IsActive.HasValue)
            {
                promotion.SetActiveStatus(request.IsActive.Value);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Promotion updated: {PromotionId}", id);
            return await _promotionRepository.GetByIdWithDetailsAsync(promotion.Id);
        }

        public async Task<bool> DeletePromotionAsync(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null || promotion.IsDeleted)
                throw new KeyNotFoundException("Promotion not found.");

            promotion.SoftDelete();
            await _context.SaveChangesAsync();

            _logger.LogInformation("Promotion soft deleted: {PromotionId}", id);
            return true;
        }

        public async Task<bool> RestorePromotionAsync(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null || !promotion.IsDeleted)
                throw new KeyNotFoundException("Promotion not found or already active.");

            promotion.Restore();
            await _context.SaveChangesAsync();

            _logger.LogInformation("Promotion restored: {PromotionId}", id);
            return true;
        }

        public async Task<IEnumerable<ProductPromotionDto>> GetPromotionProductsAsync(int promotionId)
        {
            return await _promotionRepository.GetPromotionProductsAsync(promotionId);
        }

        public async Task<ProductPromotionDto> AddProductToPromotionAsync(int promotionId, AddProductToPromotionRequest request)
        {
            var promotion = await _context.Promotions.FindAsync(promotionId);
            if (promotion == null || promotion.IsDeleted)
                throw new KeyNotFoundException("Promotion not found.");

            var carPart = await _context.CarParts.FindAsync(request.PartId);
            if (carPart == null || carPart.IsDeleted)
                throw new KeyNotFoundException("Car part not found.");

            if (await _promotionRepository.PromotionHasProductAsync(promotionId, request.PartId))
                throw new InvalidOperationException("Product already added to this promotion.");

            var productPromotion = new ProductPromotion(promotionId, request.PartId);
            await _context.ProductPromotions.AddAsync(productPromotion);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {PartId} added to promotion {PromotionId}", request.PartId, promotionId);

            return new ProductPromotionDto
            {
                Id = productPromotion.Id,
                PromotionId = productPromotion.PromotionId,
                PartId = productPromotion.PartId,
                PartName = carPart.PartName,
                PartNumber = carPart.PartNumber,
                CreatedAt = productPromotion.CreatedAt,
                UpdatedAt = productPromotion.UpdatedAt
            };
        }

        public async Task<bool> RemoveProductFromPromotionAsync(int promotionId, int partId)
        {
            var productPromotion = await _context.ProductPromotions
                .FirstOrDefaultAsync(pp => pp.PromotionId == promotionId && pp.PartId == partId);

            if (productPromotion == null)
                throw new KeyNotFoundException("Product not found in this promotion.");

            _context.ProductPromotions.Remove(productPromotion);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {PartId} removed from promotion {PromotionId}", partId, promotionId);
            return true;
        }
    }
}