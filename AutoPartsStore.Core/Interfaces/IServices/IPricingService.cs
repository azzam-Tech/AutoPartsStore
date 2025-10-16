using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.Promotions;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IPricingService
    {
        Task CalculateAndUpdateFinalPriceAsync(CarPart carPart, Promotion? promotion);
        
        // Basic calculation methods
        decimal CalculateFinalPrice(decimal unitPrice, DiscountType discountType, decimal discountValue);
        decimal CalculateFinalTotal(decimal unitPrice, DiscountType discountType, decimal discountValue, int quantity = 1);
        decimal CalculateTotalDiscount(decimal unitPrice, DiscountType discountType, decimal discountValue, int quantity = 1);
        decimal CalculateTotalPrice(decimal unitPrice, int quantity = 1);
        
        // NEW: Best price calculation considering BOTH product discount AND promotion
        decimal CalculateBestFinalPrice(decimal unitPrice, decimal discountPercent, Promotion? promotion);
        decimal CalculateBestTotalDiscount(decimal unitPrice, decimal discountPercent, Promotion? promotion, int quantity);
        decimal CalculateBestFinalTotal(decimal unitPrice, decimal discountPercent, Promotion? promotion, int quantity);
    }
}