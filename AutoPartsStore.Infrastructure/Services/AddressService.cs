using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.Address;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoPartsStore.Infrastructure.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<AddressService> _logger;

        public AddressService(IAddressRepository addressRepository, AppDbContext context, ILogger<AddressService> logger)
        {
            _addressRepository = addressRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<AddressDto>> GetUserAddressesAsync(int userId)
        {
            return await _addressRepository.GetByUserIdAsync(userId);
        }

        public async Task<AddressDto?> GetByIdAsync(int id)
        {
            return await _addressRepository.GetByIdWithDetailsAsync(id);
        }

        public async Task<AddressDto> CreateAsync(CreateAddressRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            var district = await _context.Districts
                //.Include(d => d.City)
                .FirstOrDefaultAsync(d => d.Id == request.DistrictId);
            if (district == null)
                throw new InvalidOperationException("District not found.");

            if (await _addressRepository.UserHasAddressAsync(request.UserId, request.DistrictId, request.StreetName, request.StreetNumber))
                throw new InvalidOperationException("Address already exists for this user.");

            var address = new Address(
                request.UserId,
                request.DistrictId,
                request.StreetName,
                request.StreetNumber,
                request.PostalCode
            );

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Address created for user {UserId}", request.UserId);
            var addressDto = await _addressRepository.GetByIdWithDetailsAsync(address.Id);
            if (addressDto == null)
                throw new InvalidOperationException("Failed to retrieve created address.");
            return addressDto;
        }

        public async Task<AddressDto> UpdateAsync(int id, UpdateAddressRequest request)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
                throw new KeyNotFoundException("Address not found.");

            var district = await _context.Districts.FindAsync(request.DistrictId);
            if (district == null)
                throw new InvalidOperationException("District not found.");

            if (await _addressRepository.UserHasAddressAsync(address.UserId, request.DistrictId, request.StreetName, request.StreetNumber, id))
                throw new InvalidOperationException("Address already exists for this user.");

            address.UpdateAddress(
                request.StreetName,
                request.StreetNumber,
                request.PostalCode,
                request.DistrictId
            );

            await _context.SaveChangesAsync();

            _logger.LogInformation("Address updated: {AddressId}", id);
            var addressDto = await _addressRepository.GetByIdWithDetailsAsync(address.Id);
            if (addressDto == null)
                throw new InvalidOperationException("Failed to retrieve updated address.");
            return addressDto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
                throw new KeyNotFoundException("Address not found.");

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Address deleted: {AddressId}", id);
            return true;
        }

        public async Task<bool> SetDefaultAddressAsync(int userId, int addressId)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

            if (address == null)
                throw new InvalidOperationException("Address not found for this user.");

            // Here you would typically update a IsDefault flag on the address
            // and set other addresses for this user to not default
            // For now, we'll just log the action

            _logger.LogInformation("Address {AddressId} set as default for user {UserId}", addressId, userId);
            return true;
        }
    }
}