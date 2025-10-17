using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.Extensions.Logging;

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
                decimal finalPrice;

                // RULE: Product discount has PRIORITY over promotion
                if (carPart.DiscountPercent > 0)
                {
                    // Use product discount
                    finalPrice = carPart.UnitPrice * (1 - carPart.DiscountPercent / 100);
                    _logger.LogInformation("Using product discount for part {PartId}: {Discount}%",
                        carPart.Id, carPart.DiscountPercent);
                }
                else if (promotion != null && promotion.IsActiveNow())
                {
                    // Use promotion only if no product discount
                    finalPrice = CalculatePriceWithPromotion(carPart.UnitPrice, promotion);
                    _logger.LogInformation("Using promotion for part {PartId}: {PromotionName}",
                        carPart.Id, promotion.PromotionName);
                }
                else
                {
                    // No discount
                    finalPrice = carPart.UnitPrice;
                }

                carPart.UpdateFinalPrice(finalPrice);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Final price updated for part {PartId}: {FinalPrice}",
                    carPart.Id, finalPrice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating final price for part {CarPartId}", carPart.Id);
                throw;
            }
        }

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

        /// <summary>
        /// Calculate final price per unit
        /// RULE: If product has discount, use it. Otherwise use promotion.
        /// </summary>
        public decimal CalculateFinalPrice(decimal unitPrice, decimal productDiscountPercent, Promotion? promotion)
        {
            // Product discount has priority
            if (productDiscountPercent > 0)
            {
                return unitPrice * (1 - productDiscountPercent / 100);
            }
            
            // Use promotion if no product discount
            if (promotion != null && promotion.IsActiveNow())
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
            
            // No discount
            return unitPrice;
        }

        /// <summary>
        /// Calculate final price - legacy method for backward compatibility
        /// </summary>
        public decimal CalculateFinalPrice(decimal unitPrice, DiscountType discountType, decimal discountValue)
        {
            return discountType switch
            {
                DiscountType.Percent => unitPrice * (1 - discountValue / 100),
                DiscountType.Fixed => Math.Max(unitPrice - discountValue, 0),
                _ => unitPrice
            };
        }

        /// <summary>
        /// Calculate total price (UnitPrice * Quantity) before any discount
        /// </summary>
        public decimal CalculateTotalPrice(decimal unitPrice, int quantity = 1)
        {
            return unitPrice * quantity;
        }

        /// <summary>
        /// Calculate total discount amount
        /// RULE: If product has discount, use it. Otherwise use promotion.
        /// </summary>
        public decimal CalculateTotalDiscount(
            decimal unitPrice,
            decimal productDiscountPercent,
            Promotion? promotion,
            int quantity)
        {
            decimal finalPrice = CalculateFinalPrice(unitPrice, productDiscountPercent, promotion);
            decimal totalBeforeDiscount = unitPrice * quantity;
            decimal totalAfterDiscount = finalPrice * quantity;
            
            return totalBeforeDiscount - totalAfterDiscount;
        }

        /// <summary>
        /// Calculate total discount - legacy method for backward compatibility
        /// </summary>
        public decimal CalculateTotalDiscount(
            decimal unitPrice,
            DiscountType discountType,
            decimal discountValue,
            int quantity = 1)
        {
            var discountPerUnit = discountType switch
            {
                DiscountType.Percent => unitPrice * discountValue / 100,
                DiscountType.Fixed => discountValue,
                _ => 0
            };
            return discountPerUnit * quantity;
        }

        /// <summary>
        /// Calculate final total (after discount, with quantity)
        /// RULE: If product has discount, use it. Otherwise use promotion.
        /// </summary>
        public decimal CalculateFinalTotal(
            decimal unitPrice,
            decimal productDiscountPercent,
            Promotion? promotion,
            int quantity)
        {
            decimal finalPrice = CalculateFinalPrice(unitPrice, productDiscountPercent, promotion);
            return finalPrice * quantity;
        }

        /// <summary>
        /// Calculate final total - legacy method for backward compatibility
        /// </summary>
        public decimal CalculateFinalTotal(
            decimal unitPrice,
            DiscountType discountType,
            decimal discountValue,
            int quantity = 1)
        {
            var finalUnitPrice = CalculateFinalPrice(unitPrice, discountType, discountValue);
            return finalUnitPrice * quantity;
        }
    }
}