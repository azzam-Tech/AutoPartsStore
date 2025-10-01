using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.Promotions;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IPricingService
    {
        Task CalculateAndUpdateFinalPriceAsync(CarPart carPart, Promotion? promotion);
        decimal CalculateFinalPrice(decimal unitPrice, DiscountType discountType, decimal discountValue);
        decimal CalculateFinalTotal(decimal unitPrice, DiscountType discountType, decimal discountValue, int quantity = 1);
        decimal CalculateTotalDiscount(decimal unitPrice, DiscountType discountType, decimal discountValue, int quantity = 1);
        decimal CalculateTotalPrice(decimal unitPrice, int quantity = 1);
        //Task RecalculateAllPricesAsync();
        //Task ApplyPromotionToProductAsync(int promotionId, int carPartId);
        //Task RemovePromotionFromProductAsync(int carPartId);
    }
}