using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.Promotions;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IPricingService
    {
        Task CalculateAndUpdateFinalPriceAsync(CarPart carPart, Promotion? promotion);
        //Task RecalculateAllPricesAsync();
        //Task ApplyPromotionToProductAsync(int promotionId, int carPartId);
        //Task RemovePromotionFromProductAsync(int carPartId);
    }
}