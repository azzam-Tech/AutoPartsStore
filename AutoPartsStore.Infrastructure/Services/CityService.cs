using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.City;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoPartsStore.Infrastructure.Services
{
    public class CityService : ICityService
    {
        private readonly ICityRepository _cityRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<CityService> _logger;

        public CityService(ICityRepository cityRepository, AppDbContext context, ILogger<CityService> logger)
        {
            _cityRepository = cityRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<CityDto>> GetAllAsync()
        {
            return await _cityRepository.GetAllAsync();
        }

        public async Task<CityDto> GetByIdAsync(int id)
        {
            return await _cityRepository.GetByIdWithDistrictsAsync(id);
        }

        public async Task<CityDto> CreateAsync(CreateCityRequest request)
        {
            if (await _cityRepository.CityNameExistsAsync(request.CityName))
                throw new InvalidOperationException($"City '{request.CityName}' already exists.");

            var city = new City(request.CityName);
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            _logger.LogInformation("City created: {CityName}", request.CityName);
            return await _cityRepository.GetByIdWithDistrictsAsync(city.Id);
        }

        public async Task<CityDto> UpdateAsync(int id, UpdateCityRequest request)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
                throw new KeyNotFoundException("City not found.");

            if (await _cityRepository.CityNameExistsAsync(request.CityName, id))
                throw new InvalidOperationException($"City '{request.CityName}' already exists.");

            city.UpdateName(request.CityName);
            await _context.SaveChangesAsync();

            _logger.LogInformation("City updated: {CityName}", request.CityName);
            return await _cityRepository.GetByIdWithDistrictsAsync(city.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var city = await _context.Cities
                .Include(c => c.Districts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (city == null)
                throw new KeyNotFoundException("City not found.");

            if (city.Districts.Any())
                throw new InvalidOperationException("Cannot delete city with districts.");

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();

            _logger.LogInformation("City deleted: {CityName}", city.CityName);
            return true;
        }
    }
}