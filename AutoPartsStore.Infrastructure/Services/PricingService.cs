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
                    if (promotion.IsActive || promotion.IsActiveNow())
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
                _logger.LogError(ex, "Error calculating final price for part {CarPartId}", carPart.Id);
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


        #region new methods

        //public ProductPriceDto CalculateProductPrice(decimal unitPrice, DiscountType discountType,
        //                                          decimal discountValue, int quantity = 1)
        //{
        //    var finalUnitPrice = CalculateFinalPrice(unitPrice, discountType, discountValue);
        //    var discountAmount = CalculateDiscountAmount(unitPrice, discountType, discountValue, quantity);

        //    return new ProductPriceDto
        //    {
        //        UnitPrice = unitPrice,
        //        DiscountType = discountType,
        //        DiscountValue = discountValue,
        //        FinalUnitPrice = finalUnitPrice,
        //        Quantity = quantity,
        //        TotalPrice = unitPrice * quantity,
        //        DiscountAmount = discountAmount,
        //        FinalTotal = finalUnitPrice * quantity
        //    };
        //}

        ////public ProductPriceDto CalculateProductPrice(CarPart carPart, int quantity = 1)
        ////{
        ////    // نفترض أن CarPart له خصم نسبة مئوية فقط (للتتوافق مع الكود الحالي)
        ////    return CalculateProductPrice(
        ////        carPart.UnitPrice,
        ////        DiscountType.Percent,
        ////        carPart.DiscountPercent,
        ////        quantity
        ////    );
        ////}

        ////public ProductPriceDto CalculateProductPrice(Promotion promotion, CarPart carPart, int quantity = 1)
        ////{
        ////    // استخدام خصم الترويج إذا كان نشطاً
        ////    if (promotion != null && promotion.IsActiveNow())
        ////    {
        ////        return CalculateProductPrice(
        ////            carPart.UnitPrice,
        ////            promotion.DiscountType,
        ////            promotion.DiscountValue,
        ////            quantity
        ////        );
        ////    }

        ////    // استخدام خصم المنتج العادي
        ////    return CalculateProductPrice(carPart, quantity);
        ////}

        ////public CartPriceSummaryDto CalculateCartPriceSummary(IEnumerable<CartItemPriceRequest> items)
        ////{
        ////    var itemList = items.ToList();

        ////    var totalPrice = itemList.Sum(item => item.UnitPrice * item.Quantity);
        ////    var totalDiscount = itemList.Sum(item =>
        ////        CalculateDiscountAmount(item.UnitPrice, item.DiscountType, item.DiscountValue, item.Quantity));
        ////    var finalTotal = totalPrice - totalDiscount;

        ////    return new CartPriceSummaryDto
        ////    {
        ////        TotalPrice = totalPrice,
        ////        TotalDiscount = totalDiscount,
        ////        FinalTotal = finalTotal,
        ////        TotalItems = itemList.Sum(item => item.Quantity)
        ////    };
        ////}

        //public decimal CalculateFinalPrice(decimal unitPrice, DiscountType discountType, decimal discountValue)
        //{
        //    return discountType switch
        //    {
        //        DiscountType.Percent => unitPrice * (1 - discountValue / 100),
        //        DiscountType.Fixed => Math.Max(unitPrice - discountValue, 0), // لا يقل عن صفر
        //        _ => unitPrice
        //    };
        //}

        //public decimal CalculateDiscountAmount(decimal unitPrice, DiscountType discountType,
        //                                     decimal discountValue, int quantity = 1)
        //{
        //    var discountPerUnit = discountType switch
        //    {
        //        DiscountType.Percent => unitPrice * discountValue / 100,
        //        DiscountType.Fixed => discountValue,
        //        _ => 0
        //    };

        //    return discountPerUnit * quantity;
        //}

        //// دوال مساعدة للتوافق مع الكود الحالي
        //public decimal CalculateFinalPrice(decimal unitPrice, decimal discountPercent)
        //{
        //    return CalculateFinalPrice(unitPrice, DiscountType.Percent, discountPercent);
        //}

        //public decimal CalculateDiscountAmount(decimal unitPrice, decimal discountPercent, int quantity = 1)
        //{
        //    return CalculateDiscountAmount(unitPrice, DiscountType.Percent, discountPercent, quantity);
        //}

        #endregion

        public decimal CalculateFinalPrice(decimal unitPrice, DiscountType discountType, decimal discountValue)
        {
            return discountType switch
            {
                DiscountType.Percent => unitPrice * (1 - discountValue / 100),
                DiscountType.Fixed => Math.Max(unitPrice - discountValue, 0), // لا يقل عن صفر
                _ => unitPrice
            };
        }

        public decimal CalculateTotalPrice(decimal unitPrice, int quantity = 1)
        {
            return unitPrice * quantity;
        }

        public decimal CalculateTotalDiscount(decimal unitPrice, DiscountType discountType,
                                       decimal discountValue, int quantity = 1)
        {
            var discountPerUnit = discountType switch
            {
                DiscountType.Percent => unitPrice * discountValue / 100,
                DiscountType.Fixed => discountValue,
                _ => 0
            };
            return discountPerUnit * quantity;
        }

        public decimal CalculateFinalTotal(decimal unitPrice, DiscountType discountType,
                                       decimal discountValue, int quantity = 1)
        {
            var finalUnitPrice = CalculateFinalPrice(unitPrice, discountType, discountValue);
            return finalUnitPrice * quantity;
        }

    }
}