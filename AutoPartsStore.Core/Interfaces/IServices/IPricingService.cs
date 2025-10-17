using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.Promotions;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IPricingService
    {
        Task CalculateAndUpdateFinalPriceAsync(CarPart carPart, Promotion? promotion);
        
        // Unified pricing methods (Product discount has PRIORITY over promotion)
        decimal CalculateFinalPrice(decimal unitPrice, decimal productDiscountPercent, Promotion? promotion);
        decimal CalculateTotalPrice(decimal unitPrice, int quantity = 1);
        decimal CalculateTotalDiscount(decimal unitPrice, decimal productDiscountPercent, Promotion? promotion, int quantity);
        decimal CalculateFinalTotal(decimal unitPrice, decimal productDiscountPercent, Promotion? promotion, int quantity);
        
        // Legacy methods for backward compatibility
        decimal CalculateFinalPrice(decimal unitPrice, DiscountType discountType, decimal discountValue);
        decimal CalculateFinalTotal(decimal unitPrice, DiscountType discountType, decimal discountValue, int quantity = 1);
        decimal CalculateTotalDiscount(decimal unitPrice, DiscountType discountType, decimal discountValue, int quantity = 1);
    }
}