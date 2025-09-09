using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.District;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoPartsStore.Web.Controllers
{
    [ApiController]
    [Route("api/districts")]
    public class DistrictsController : BaseController
    {
        private readonly IDistrictService _districtService;
        private readonly ILogger<DistrictsController> _logger;

        public DistrictsController(IDistrictService districtService, ILogger<DistrictsController> logger)
        {
            _districtService = districtService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var districts = await _districtService.GetAllAsync();
            return Success(districts);
        }

        [HttpGet("city/{cityId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCityId(int cityId)
        {
            var districts = await _districtService.GetByCityIdAsync(cityId);
            return Success(districts);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var district = await _districtService.GetByIdAsync(id);
            return district != null ? Success(district) : NotFound();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateDistrictRequest request)
        {
            try
            {
                var district = await _districtService.CreateAsync(request);
                return Success(district, "District created successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDistrictRequest request)
        {
            try
            {
                var district = await _districtService.UpdateAsync(id, request);
                return Success(district, "District updated successfully");
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
                await _districtService.DeleteAsync(id);
                return Success("District deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}