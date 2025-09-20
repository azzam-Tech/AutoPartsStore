using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.Promotions;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace AutoPartsStore.Infrastructure.Services
{
    public class PricingService : IPricingService
    {
        private readonly AppDbContext _context;
        private readonly IPromotionRepository _promotionRepository;
        private readonly ILogger<PricingService> _logger;

        public PricingService(
            AppDbContext context,
            IPromotionRepository promotionRepository,
            ILogger<PricingService> logger)
        {
            _context = context;
            _promotionRepository = promotionRepository;
            _logger = logger;
        }

        public async Task CalculateAndUpdateFinalPriceAsync(CarPart carPart, Promotion? promotion)
        {
            try
            {
                decimal finalPrice = 0;
                
                if (carPart.DiscountPercent > 0)
                {
                    // الأولوية للخصم الخاص بالمنتج
                    finalPrice = carPart.UnitPrice * (1 - carPart.DiscountPercent / 100);
                    _logger.LogInformation("Using product discount for part {PartId}: {Discount}%",
                        carPart.Id, carPart.DiscountPercent);
                }
                else if (promotion == null)
                {
                    if (carPart.DiscountPercent > 0)
                    {
                        // الأولوية للخصم الخاص بالمنتج
                        finalPrice = carPart.UnitPrice * (1 - carPart.DiscountPercent / 100);
                        _logger.LogInformation("Using product discount for part {PartId}: {Discount}%",
                            carPart.Id, carPart.DiscountPercent);
                    }
                    else
                    {
                        finalPrice = carPart.UnitPrice;
                    }
                }
                else
                {
                    if(promotion.IsActive || promotion.IsActiveNow())
                    {
                        finalPrice = CalculatePriceWithPromotion(carPart.UnitPrice, promotion);
                        carPart.UpdateFinalPrice(finalPrice);
                        _logger.LogInformation("Using promotion for part {PartId}: {PromotionValue}",
                                  carPart.Id, finalPrice);
                    }
                    else
                    {
                        finalPrice = carPart.UnitPrice;
                    }

                }

                carPart.UpdateFinalPrice(finalPrice);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Final price updated for part {PartId}: {FinalPrice}",
                    carPart.Id, finalPrice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating final price for part {CarPartId}",carPart.Id);
                throw;
            }
        }

        //public async Task RecalculateAllPricesAsync()
        //{
        //    try
        //    {
        //        var carPartIds = await _context.CarParts
        //            .Where(p => !p.IsDeleted)
        //            .Select(p => p.Id)
        //            .ToListAsync();

        //        foreach (var partId in carPartIds)
        //        {
        //            await CalculateAndUpdateFinalPriceAsync(partId);
        //        }

        //        _logger.LogInformation("Recalculated prices for {Count} car parts", carPartIds.Count);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error recalculating all prices");
        //        throw;
        //    }
        //}

        //public async Task ApplyPromotionToProductAsync(int promotionId, int carPartId)
        //{
        //    try
        //    {
        //        // التحقق من وجود العرض والمنتج
        //        var promotion = await _promotionRepository.GetByIdAsync(promotionId);
        //        var carPart = await _context.CarParts.FindAsync(carPartId);

        //        if (promotion == null || promotion.IsDeleted)
        //            throw new ArgumentException("Promotion not found or deleted");

        //        if (carPart == null || carPart.IsDeleted)
        //            throw new ArgumentException("Car part not found or deleted");

        //        await CalculateAndUpdateFinalPriceAsync(carPartId);
        //        _logger.LogInformation("Applied promotion {PromotionId} to product {PartId}",
        //            promotionId, carPartId);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error applying promotion {PromotionId} to product {PartId}",
        //            promotionId, carPartId);
        //        throw;
        //    }
        //}

        //public async Task RemovePromotionFromProductAsync(int carPartId)
        //{
        //    try
        //    {
        //        await CalculateAndUpdateFinalPriceAsync(carPartId);
        //        _logger.LogInformation("Removed promotions from product {PartId}", carPartId);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error removing promotions from product {PartId}", carPartId);
        //        throw;
        //    }
        //}



        private decimal CalculatePriceWithPromotion(decimal unitPrice, Promotion promotion)
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