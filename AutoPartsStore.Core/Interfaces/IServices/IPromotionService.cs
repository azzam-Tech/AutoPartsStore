using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Models;
using AutoPartsStore.Core.Models.Promotions;

namespace AutoPartsStore.Core.Interfaces
{
    public interface IPromotionService
    {
        Task<PagedResult<PromotionDto>> GetAllPromotionsAsync(PromotionFilter filter);
        Task<PromotionDto> GetPromotionByIdAsync(int id);
        Task<IEnumerable<PromotionDto>> GetActivePromotionsAsync();
        Task<PromotionDto> CreatePromotionAsync(CreatePromotionRequest request);
        Task<PromotionDto> UpdatePromotionAsync(int id, UpdatePromotionRequest request);
        Task<bool> DeletePromotionAsync(int id);
        Task<bool> RestorePromotionAsync(int id);
        Task<IEnumerable<ProductPromotionDto>> GetPromotionProductsAsync(int promotionId);
        Task<ProductPromotionDto> AddProductToPromotionAsync(int promotionId, AddProductToPromotionRequest request);
        Task<bool> RemoveProductFromPromotionAsync(int promotionId, int partId);
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
    }
}