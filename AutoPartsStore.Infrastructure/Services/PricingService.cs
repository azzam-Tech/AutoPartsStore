using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.Promotions;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoPartsStore.Infrastructure.Services
{
    public class PricingService : IPricingService
    {
        private readonly AppDbContext _context;
        private readonly IProductPromotionRepository _productPromotionRepo;
        private readonly IPromotionRepository _promotionRepository;
        private readonly ILogger<PricingService> _logger;

        public PricingService(
            AppDbContext context,
            IProductPromotionRepository productPromotionRepo,
            IPromotionRepository promotionRepository,
            ILogger<PricingService> logger)
        {
            _context = context;
            _productPromotionRepo = productPromotionRepo;
            _promotionRepository = promotionRepository;
            _logger = logger;
        }

        public async Task CalculateAndUpdateFinalPriceAsync(int carPartId)
        {
            try
            {
                var carPart = await _context.CarParts
                    .FirstOrDefaultAsync(p => p.Id == carPartId && !p.IsDeleted);

                if (carPart == null)
                {
                    _logger.LogWarning("Car part with ID {CarPartId} not found or deleted", carPartId);
                    return;
                }

                // الحصول على أفضل عرض فعال للمنتج
                var bestPromotion = await GetBestActivePromotionForProductAsync(carPartId);

                decimal finalPrice;

                if (carPart.DiscountPercent > 0)
                {
                    // الأولوية للخصم الخاص بالمنتج
                    finalPrice = carPart.UnitPrice * (1 - carPart.DiscountPercent / 100);
                    _logger.LogInformation("Using product discount for part {PartId}: {Discount}%",
                        carPartId, carPart.DiscountPercent);
                }
                else if (bestPromotion != null)
                {
                    // تطبيق عرض التخفيض العام
                    finalPrice = bestPromotion.Value;
                    _logger.LogInformation("Using promotion for part {PartId}: {PromotionValue}",
                        carPartId, bestPromotion.Value);
                }
                else
                {
                    // لا يوجد عروض
                    finalPrice = carPart.UnitPrice;
                    _logger.LogInformation("No discounts applied for part {PartId}", carPartId);
                }

                carPart.UpdateFinalPrice(finalPrice);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Final price updated for part {PartId}: {FinalPrice}",
                    carPartId, finalPrice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating final price for part {CarPartId}", carPartId);
                throw;
            }
        }

        public async Task RecalculateAllPricesAsync()
        {
            try
            {
                var carPartIds = await _context.CarParts
                    .Where(p => !p.IsDeleted)
                    .Select(p => p.Id)
                    .ToListAsync();

                foreach (var partId in carPartIds)
                {
                    await CalculateAndUpdateFinalPriceAsync(partId);
                }

                _logger.LogInformation("Recalculated prices for {Count} car parts", carPartIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recalculating all prices");
                throw;
            }
        }

        public async Task ApplyPromotionToProductAsync(int promotionId, int carPartId)
        {
            try
            {
                // التحقق من وجود العرض والمنتج
                var promotion = await _promotionRepository.GetByIdAsync(promotionId);
                var carPart = await _context.CarParts.FindAsync(carPartId);

                if (promotion == null || promotion.IsDeleted)
                    throw new ArgumentException("Promotion not found or deleted");

                if (carPart == null || carPart.IsDeleted)
                    throw new ArgumentException("Car part not found or deleted");

                await CalculateAndUpdateFinalPriceAsync(carPartId);
                _logger.LogInformation("Applied promotion {PromotionId} to product {PartId}",
                    promotionId, carPartId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying promotion {PromotionId} to product {PartId}",
                    promotionId, carPartId);
                throw;
            }
        }

        public async Task RemovePromotionFromProductAsync(int carPartId)
        {
            try
            {
                await CalculateAndUpdateFinalPriceAsync(carPartId);
                _logger.LogInformation("Removed promotions from product {PartId}", carPartId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing promotions from product {PartId}", carPartId);
                throw;
            }
        }

        public async Task<decimal?> GetBestActivePromotionForProductAsync(int carPartId)
        {
            try
            {
                var part = await _context.CarParts.FindAsync(carPartId);
                // جلب جميع العروض النشطة المرتبطة بهذه القطعة
                var activePromotions = await _context.ProductPromotions
                    .Where(pp => pp.PartId == carPartId)
                    .Select(pp => pp.Promotion)
                    .Where(p => p.IsActive && !p.IsDeleted && p.IsActiveNow())
                    .ToListAsync();

                decimal? finalPrice = null; // السعر الأصلي هو الأساس

                // إذا وجدت عروض، نحسب السعر النهائي لكل واحد ونختار الأفضل (الأقل سعرًا)
                if (activePromotions.Any())
                {
                    var bestPrice = part.UnitPrice; // نبدأ بالسعر الأصلي

                    foreach (var promo in activePromotions)
                    {
                        decimal priceAfterPromo = part.UnitPrice;

                        if (promo.DiscountType == DiscountType.Percent)
                        {
                            priceAfterPromo = part.UnitPrice * (1 - promo.DiscountValue / 100);
                        }
                        else if (promo.DiscountType == DiscountType.Fixed)
                        {
                            priceAfterPromo = Math.Max(0, part.UnitPrice - promo.DiscountValue);
                        }

                        // نختار السعر الأقل (الأفضل للمستخدم)
                        if (priceAfterPromo < bestPrice)
                        {
                            bestPrice = priceAfterPromo;
                        }
                    }

                    finalPrice = bestPrice;
                }

                return finalPrice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting best promotion for product {PartId}", carPartId);
                return null;
            }
        }

        private decimal CalculatePriceWithPromotion(decimal unitPrice, PromotionDto promotion)
        {
            if (promotion.DiscountType == DiscountType.Percent)
            {
                return unitPrice * (1 - promotion.DiscountValue / 100);
            }
            else
            {
                return Math.Max(0, unitPrice - promotion.DiscountValue);
            }
        }
    }
}