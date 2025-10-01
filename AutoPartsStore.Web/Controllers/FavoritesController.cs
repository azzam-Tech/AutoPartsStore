using AutoPartsStore.Core.Interfaces.IServices;
using AutoPartsStore.Core.Models.Favorites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoPartsStore.Web.Controllers
{
    [ApiController]
    [Route("api/favorites")]
    [Authorize]
    public class FavoritesController : BaseController
    {
        private readonly IFavoriteService _favoriteService;
        private readonly ILogger<FavoritesController> _logger;

        public FavoritesController(
            IFavoriteService favoriteService,
            ILogger<FavoritesController> logger)
        {
            _favoriteService = favoriteService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = GetAuthenticatedUserId();
            var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
            return Success(favorites);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetFavoriteCount()
        {
            var userId = GetAuthenticatedUserId();
            var count = await _favoriteService.GetFavoriteCountAsync(userId);
            return Success(new { count });
        }

        [HttpPost]
        public async Task<IActionResult> AddToFavorites([FromBody] AddToFavoriteRequest request)
        {
            var userId = GetAuthenticatedUserId();

            try
            {
                await _favoriteService.AddToFavoritesAsync(userId, request);
                return Success("Product added to favorites successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{partId}")]
        public async Task<IActionResult> RemoveFromFavorites(int partId)
        {
            var userId = GetAuthenticatedUserId();

            try
            {
                await _favoriteService.RemoveFromFavoritesAsync(userId, partId);
                return Success("Product removed from favorites successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("check/{partId}")]
        public async Task<IActionResult> CheckFavorite(int partId)
        {
            var userId = GetAuthenticatedUserId();

            try
            {
                var isFavorite = await _favoriteService.IsProductInFavoritesAsync(userId, partId);
                return Success(new { isFavorite });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new InvalidOperationException("Authenticated user ID claim is missing.");
            }
            return int.Parse(userIdClaim);
        }
    }
}