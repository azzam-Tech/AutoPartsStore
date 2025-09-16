using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.Promotions;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IProductPromotionRepository : IBaseRepository<ProductPromotion>
    {
        Task<IEnumerable<ProductPromotionDto>> GetByPromotionIdAsync(int promotionId);
        Task<IEnumerable<ProductPromotionDto>> GetByPartIdAsync(int partId);
        Task<bool> ProductHasActivePromotionAsync(int partId);
    }
}