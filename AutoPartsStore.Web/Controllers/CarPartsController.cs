using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.CarPart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoPartsStore.Web.Controllers
{
    [ApiController]
    [Route("api/car-parts")]
    public class CarPartsController : BaseController
    {
        private readonly ICarPartService _partService;
        private readonly ILogger<CarPartsController> _logger;

        public CarPartsController(ICarPartService partService, ILogger<CarPartsController> logger)
        {
            _partService = partService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetFiltered([FromQuery] CarPartFilter filter)
        {
            var parts = await _partService.GetFilteredAsync(filter);
            return Success(parts);
        }

        [HttpGet("featured")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFeatured([FromQuery] int count = 8)
        {
            var parts = await _partService.GetFeaturedAsync(count);
            return Success(parts);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var part = await _partService.GetByIdAsync(id);
            return part != null ? Success(part) : NotFound();
        }

        [HttpGet("max-price")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMaxPrice()
        {
            var maxPrice = await _partService.GetMaxPriceAsync();
            return Success(maxPrice);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Supplier")]
        public async Task<IActionResult> Create([FromBody] CreateCarPartRequest request)
        {
            try
            {
                var part = await _partService.CreateAsync(request);
                return Success(part, "Car part created successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Supplier")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCarPartRequest request)
        {
            try
            {
                var part = await _partService.UpdateAsync(id, request);
                return Success(part, "Car part updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _partService.DeleteAsync(id);
                return Success("Car part deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/stock")]
        [Authorize(Roles = "Admin,Supplier")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] int quantity)
        {
            try
            {
                await _partService.UpdateStockAsync(id, quantity);
                return Success("Stock updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/price")]
        [Authorize(Roles = "Admin,Supplier")]
        public async Task<IActionResult> UpdatePrice(int id, [FromBody] decimal price)
        {
            try
            {
                await _partService.UpdatePriceAsync(id, price);
                return Success("Price updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}