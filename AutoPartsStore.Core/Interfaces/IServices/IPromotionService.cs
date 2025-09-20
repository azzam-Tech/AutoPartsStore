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
        Task<IEnumerable<PartPromotionDto>> GetPromotionProductsAsync(int promotionId);
        Task<BulkOperationResult> AssignPromotionToProductsAsync(int promotionId, List<int> ProductIds);
        Task<BulkOperationResult> RemovePromotionFromProductsAsync(List<int> ProductIds);
        Task<BulkOperationResult> ReplacePromotionForProductsAsync(int? newPromotionId, List<int> carPartIds);
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
    }
}