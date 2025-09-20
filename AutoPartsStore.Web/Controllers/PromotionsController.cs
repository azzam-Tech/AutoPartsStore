using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.Promotions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoPartsStore.Web.Controllers
{
    [ApiController]
    [Route("api/promotions")]
    [Authorize(Roles = "Admin")]
    public class PromotionsController : BaseController
    {
        private readonly IPromotionService _promotionService;
        private readonly ILogger<PromotionsController> _logger;

        public PromotionsController(IPromotionService promotionService, ILogger<PromotionsController> logger)
        {
            _promotionService = promotionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPromotions([FromQuery] PromotionFilter filter)
        {
            var promotions = await _promotionService.GetAllPromotionsAsync(filter);
            return Success(promotions);
        }

        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActivePromotions()
        {
            var promotions = await _promotionService.GetActivePromotionsAsync();
            return Success(promotions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPromotionById(int id)
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(id);
            return promotion != null ? Success(promotion) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionRequest request)
        {
            try
            {
                var promotion = await _promotionService.CreatePromotionAsync(request);
                return Success(promotion, "Promotion created successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePromotion(int id, [FromBody] UpdatePromotionRequest request)
        {
            try
            {
                var promotion = await _promotionService.UpdatePromotionAsync(id, request);
                return Success(promotion, "Promotion updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePromotion(int id)
        {
            try
            {
                await _promotionService.DeletePromotionAsync(id);
                return Success("Promotion deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> RestorePromotion(int id)
        {
            try
            {
                await _promotionService.RestorePromotionAsync(id);
                return Success("Promotion restored successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/active")]
        [Authorize(Roles = "Admin,Supplier")]
        public async Task<IActionResult> ActivateAsync(int id)
        {
            try
            {
                await _promotionService.ActivateAsync(id);
                return Success("promotion activated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/inactive")]
        [Authorize(Roles = "Admin,Supplier")]
        public async Task<IActionResult> DeactivateAsync(int id)
        {
            try
            {
                await _promotionService.DeactivateAsync(id);
                return Success("promotion deactivated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{promotionId}/products")]
        public async Task<IActionResult> GetPromotionProducts(int promotionId)
        {
            try
            {
                var products = await _promotionService.GetPromotionProductsAsync(promotionId);
                return Success(products);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{promotionId}/products")]
        public async Task<IActionResult> AddProductToPromotion(int promotionId, List<int> ProductIds)
        {
            try
            {
                var result = await _promotionService.AssignPromotionToProductsAsync(promotionId, ProductIds);
                return Success(result, "Product added to promotion successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{promotionId}/products")]
        public async Task<IActionResult> RemoveProductFromPromotion(List<int> ProductIds)
        {
            try
            {
                await _promotionService.RemovePromotionFromProductsAsync(ProductIds);
                return Success("Product removed from promotion successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("products/bulk/replace")]
        public async Task<IActionResult> ReplacePromotionForProducts(
            [FromQuery] int? newPromotionId,
            [FromBody] List<int> carPartIds)
        {
            if (carPartIds == null || !carPartIds.Any())
                return BadRequest("No car parts specified");

            var result = await _promotionService.ReplacePromotionForProductsAsync(newPromotionId, carPartIds);

            var message = newPromotionId.HasValue
                ? $"Replaced promotion for {result.SuccessCount} products"
                : $"Removed promotion from {result.SuccessCount} products";

            return Success(result, message);
        }
    }
}