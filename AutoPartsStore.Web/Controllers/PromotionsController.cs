using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.Promotion;
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
        public async Task<IActionResult> GetAllPromotions()
        {
            var promotions = await _promotionService.GetAllPromotionsAsync();
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
        public async Task<IActionResult> AddProductToPromotion(int promotionId, [FromBody] AddProductToPromotionRequest request)
        {
            try
            {
                var result = await _promotionService.AddProductToPromotionAsync(promotionId, request);
                return Success(result, "Product added to promotion successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{promotionId}/products/{partId}")]
        public async Task<IActionResult> RemoveProductFromPromotion(int promotionId, int partId)
        {
            try
            {
                await _promotionService.RemoveProductFromPromotionAsync(promotionId, partId);
                return Success("Product removed from promotion successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}