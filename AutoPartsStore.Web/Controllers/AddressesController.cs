using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.Address;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoPartsStore.Web.Controllers
{
    [ApiController]
    [Route("api/addresses")]
    public class AddressesController : BaseController
    {
        private readonly IAddressService _addressService;
        private readonly ILogger<AddressesController> _logger;

        public AddressesController(IAddressService addressService, ILogger<AddressesController> logger)
        {
            _addressService = addressService;
            _logger = logger;
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserAddresses(int userId)
        {
            // Verify the authenticated user can only access their own addresses
            var authenticatedUserId = GetAuthenticatedUserId();
            if (authenticatedUserId != userId && !User.IsInRole("Admin"))
                return Forbidden();

            var addresses = await _addressService.GetUserAddressesAsync(userId);
            return Success(addresses);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var address = await _addressService.GetByIdAsync(id);

            // Verify the authenticated user can only access their own address
            var authenticatedUserId = GetAuthenticatedUserId();
            if (address != null && address.UserId != authenticatedUserId && !User.IsInRole("Admin"))
                return Forbidden();

            return address != null ? Success(address) : NotFound();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateAddressRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify the authenticated user can only create addresses for themselves
            var authenticatedUserId = GetAuthenticatedUserId();
            if (request.UserId != authenticatedUserId && !User.IsInRole("Admin"))
                return Forbidden();

            try
            {
                var address = await _addressService.CreateAsync(request);
                return Success(address, "Address created successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAddressRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var address = await _addressService.GetByIdAsync(id);
            if (address == null)
                return NotFound();

            // Verify the authenticated user can only update their own address
            var authenticatedUserId = GetAuthenticatedUserId();
            if (address.UserId != authenticatedUserId && !User.IsInRole("Admin"))
                return Forbidden();

            try
            {
                var updatedAddress = await _addressService.UpdateAsync(id, request);
                return Success(updatedAddress, "Address updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var address = await _addressService.GetByIdAsync(id);
            if (address == null)
                return NotFound();

            // Verify the authenticated user can only delete their own address
            var authenticatedUserId = GetAuthenticatedUserId();
            if (address.UserId != authenticatedUserId && !User.IsInRole("Admin"))
                return Forbid();

            try
            {
                await _addressService.DeleteAsync(id);
                return Success("Address deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/default")]
        [Authorize]
        public async Task<IActionResult> SetDefault(int id)
        {
            var address = await _addressService.GetByIdAsync(id);
            if (address == null)
                return NotFound();

            // Verify the authenticated user can only set their own address as default
            var authenticatedUserId = GetAuthenticatedUserId();
            if (address.UserId != authenticatedUserId && !User.IsInRole("Admin"))
                return Forbid();

            try
            {
                await _addressService.SetDefaultAddressAsync(address.UserId, id);
                return Success("Address set as default successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User ID claim not found");
            return int.Parse(userIdClaim);
        }
    }
}