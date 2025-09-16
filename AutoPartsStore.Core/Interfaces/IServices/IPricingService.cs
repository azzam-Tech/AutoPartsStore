using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.Promotions;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IPricingService
    {
        Task CalculateAndUpdateFinalPriceAsync(int carPartId);
        Task RecalculateAllPricesAsync();
        Task ApplyPromotionToProductAsync(int promotionId, int carPartId);
        Task RemovePromotionFromProductAsync(int carPartId);
        Task<decimal?> GetBestActivePromotionForProductAsync(int carPartId);
    }
}