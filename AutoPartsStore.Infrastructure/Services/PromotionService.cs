using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models;
using AutoPartsStore.Core.Models.Promotions;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoPartsStore.Infrastructure.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IPricingService _pricingService;
        private readonly AppDbContext _context;
        private readonly ILogger<PromotionService> _logger;

        public PromotionService(
            IPromotionRepository promotionRepository,
            IPricingService pricingService,
            AppDbContext context,
            ILogger<PromotionService> logger)
        {
            _promotionRepository = promotionRepository;
            _pricingService = pricingService;
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<PromotionDto>> GetAllPromotionsAsync(PromotionFilter filter)
        {
            return await _promotionRepository.GetAllWithDetailsAsync(filter);
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

        public async Task<IEnumerable<PartPromotionDto>> GetPromotionProductsAsync(int promotionId)
        {
            return await _promotionRepository.GetPromotionProductsAsync(promotionId);
        }

        public async Task<BulkOperationResult> AssignPromotionToProductsAsync(int promotionId, List<int> carPartIds)
        {
            var result = new BulkOperationResult { TotalCount = carPartIds.Count };

            // التحقق من وجود العرض
            var promotion = await _promotionRepository.GetByIdAsync(promotionId);
            if (promotion == null || promotion.IsDeleted)
            {
                foreach (var carPartId in carPartIds)
                {
                    result.Errors.Add(new BulkOperationError
                    {
                        CarPartId = carPartId,
                        ErrorMessage = "Promotion not found or deleted"
                    });
                }
                result.FailedCount = carPartIds.Count;
                return result;
            }

            foreach (var carPartId in carPartIds)
            {
                try
                {
                    var carPart = await _context.CarParts.FindAsync(carPartId);
                    if (carPart == null || carPart.IsDeleted)
                    {
                        result.Errors.Add(new BulkOperationError
                        {
                            CarPartId = carPartId,
                            ErrorMessage = "Car part not found or deleted"
                        });
                        result.FailedCount++;
                        continue;
                    }

                    carPart.AssignPromotion(promotionId);
                    await _pricingService.CalculateAndUpdateFinalPriceAsync(carPart , promotion);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new BulkOperationError
                    {
                        CarPartId = carPartId,
                        ErrorMessage = ex.Message
                    });
                    result.FailedCount++;
                }
            }

            await _context.SaveChangesAsync();
            return result;
        }


        public async Task<BulkOperationResult> RemovePromotionFromProductsAsync(List<int> carPartIds)
        {
            var result = new BulkOperationResult { TotalCount = carPartIds.Count };

            foreach (var carPartId in carPartIds)
            {
                try
                {
                    var carPart = await _context.CarParts.FindAsync(carPartId);
                    if (carPart == null || carPart.IsDeleted)
                    {
                        result.Errors.Add(new BulkOperationError
                        {
                            CarPartId = carPartId,
                            ErrorMessage = "Car part not found or deleted"
                        });
                        result.FailedCount++;
                        continue;
                    }

                    carPart.AssignPromotion(null);
                    await _pricingService.CalculateAndUpdateFinalPriceAsync(carPart, null);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new BulkOperationError
                    {
                        CarPartId = carPartId,
                        ErrorMessage = ex.Message
                    });
                    result.FailedCount++;
                }
            }

            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<BulkOperationResult> ReplacePromotionForProductsAsync(int? newPromotionId, List<int> carPartIds)
        {
            var result = new BulkOperationResult { TotalCount = carPartIds.Count };
            var promotion2 = await _promotionRepository.GetByIdAsync(newPromotionId.Value);


            // التحقق من العرض الجديد إذا كان غير null
            if (newPromotionId.HasValue)
            {
                var promotion = await _promotionRepository.GetByIdAsync(newPromotionId.Value);
                if (promotion == null || promotion.IsDeleted)
                {
                    foreach (var carPartId in carPartIds)
                    {
                        result.Errors.Add(new BulkOperationError
                        {
                            CarPartId = carPartId,
                            ErrorMessage = "New promotion not found or deleted"
                        });
                    }
                    result.FailedCount = carPartIds.Count;
                    return result;
                }
            }

            foreach (var carPartId in carPartIds)
            {
                try
                {
                    var carPart = await _context.CarParts.FindAsync(carPartId);
                    if (carPart == null || carPart.IsDeleted)
                    {
                        result.Errors.Add(new BulkOperationError
                        {
                            CarPartId = carPartId,
                            ErrorMessage = "Car part not found or deleted"
                        });
                        result.FailedCount++;
                        continue;
                    }

                    carPart.AssignPromotion(newPromotionId);
                    await _pricingService.CalculateAndUpdateFinalPriceAsync(carPart , promotion2);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new BulkOperationError
                    {
                        CarPartId = carPartId,
                        ErrorMessage = ex.Message
                    });
                    result.FailedCount++;
                }
            }

            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null || promotion.IsDeleted || !promotion.IsActive)
                throw new KeyNotFoundException("promotion not found or already inactive.");

            promotion.Deactivate();
            await _context.SaveChangesAsync();

            _logger.LogInformation("promotion deactivated: {promotionId}", id);
            return true;
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null || promotion.IsDeleted || promotion.IsActive)
                throw new KeyNotFoundException("promotion not found or already active.");
            if (DateTime.UtcNow < promotion.StartDate || DateTime.UtcNow > promotion.EndDate)
                throw new InvalidOperationException("Cannot activate promotion outside its valid date range.");

            promotion.Activate();
            await _context.SaveChangesAsync();

            _logger.LogInformation("promotion activated: {promotionId}", id);
            return true;
        }
    }
}