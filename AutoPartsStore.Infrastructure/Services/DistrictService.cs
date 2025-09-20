using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.District;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoPartsStore.Infrastructure.Services
{
    public class DistrictService : IDistrictService
    {
        private readonly IDistrictRepository _districtRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<DistrictService> _logger;

        public DistrictService(IDistrictRepository districtRepository, AppDbContext context, ILogger<DistrictService> logger)
        {
            _districtRepository = districtRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<DistrictDto>> GetAllAsync()
        {
            return await _districtRepository.GetAllAsync();
        }

        public async Task<IEnumerable<DistrictDto>> GetByCityIdAsync(int cityId)
        {
            return await _districtRepository.GetByCityIdAsync(cityId);
        }

        public async Task<DistrictDto> GetByIdAsync(int id)
        {
            return await _districtRepository.GetByIdWithDetailsAsync(id);
        }

        public async Task<DistrictDto> CreateAsync(CreateDistrictRequest request)
        {
            if (await _districtRepository.DistrictExistsAsync(request.CityId, request.DistrictName))
                throw new InvalidOperationException($"District '{request.DistrictName}' already exists in this city.");

            var city = await _context.Cities.FindAsync(request.CityId);
            if (city == null)
                throw new InvalidOperationException("City not found.");

            var district = new District(request.CityId, request.DistrictName);
            _context.Districts.Add(district);
            await _context.SaveChangesAsync();

            _logger.LogInformation("District created: {DistrictName} in city {CityId}",
                request.DistrictName, request.CityId);

            return await _districtRepository.GetByIdWithDetailsAsync(district.Id);
        }

        public async Task<DistrictDto> UpdateAsync(int id, UpdateDistrictRequest request)
        {
            var district = await _context.Districts.FindAsync(id);
            if (district == null)
                throw new KeyNotFoundException("District not found.");

            if (await _districtRepository.DistrictExistsAsync(request.CityId, request.DistrictName, id))
                throw new InvalidOperationException($"District '{request.DistrictName}' already exists in this city.");

            var city = await _context.Cities.FindAsync(request.CityId);
            if (city == null)
                throw new InvalidOperationException("City not found.");

            district.UpdateName(request.DistrictName);
            district.ChangeCity(request.CityId);

            await _context.SaveChangesAsync();

            _logger.LogInformation("District updated: {DistrictName} in city {CityId}",
                request.DistrictName, request.CityId);

            return await _districtRepository.GetByIdWithDetailsAsync(district.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var district = await _context.Districts
                .Include(d => d.Addresses)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (district == null)
                throw new KeyNotFoundException("District not found.");

            if (district.Addresses.Any())
                throw new InvalidOperationException("Cannot delete district with addresses.");

            _context.Districts.Remove(district);
            await _context.SaveChangesAsync();

            _logger.LogInformation("District deleted: {DistrictName}", district.DistrictName);
            return true;
        }
    }
}