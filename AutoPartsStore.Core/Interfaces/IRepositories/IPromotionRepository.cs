using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models.Promotion;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IPromotionRepository : IBaseRepository<Promotion>
    {
        Task<IEnumerable<PromotionDto>> GetAllWithDetailsAsync();
        Task<PromotionDto> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<PromotionDto>> GetActivePromotionsAsync();
        Task<IEnumerable<ProductPromotionDto>> GetPromotionProductsAsync(int promotionId);
        Task<bool> PromotionHasProductAsync(int promotionId, int partId);
    }
}