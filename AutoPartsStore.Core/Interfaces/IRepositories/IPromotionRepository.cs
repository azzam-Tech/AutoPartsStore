using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models;
using AutoPartsStore.Core.Models.Promotions;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IPromotionRepository : IBaseRepository<Promotion>
    {
        Task<PagedResult<PromotionDto>> GetAllWithDetailsAsync(PromotionFilter filter);
        Task<PromotionDto> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<PromotionDto>> GetActivePromotionsAsync();
        Task<IEnumerable<PartPromotionDto>> GetPromotionProductsAsync(int promotionId);
        Task<bool> PromotionHasProductAsync(int promotionId, int partId);
    }
}