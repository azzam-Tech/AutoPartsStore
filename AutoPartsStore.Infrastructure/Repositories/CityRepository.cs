using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.City;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsStore.Infrastructure.Repositories
{
    public class CityRepository : BaseRepository<City>, ICityRepository
    {
        public CityRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<CityDto>> GetAllCitiesAsync()
        {
            return await _context.Cities
                .Select(c => new CityDto
                {
                    Id = c.Id,
                    CityName = c.CityName,
                    DistrictsCount = c.Districts.Count
                })
                .OrderBy(c => c.CityName)
                .ToListAsync();
        }

        public async Task<CityDto?> GetByIdWithDistrictsAsync(int id)
        {
            return await _context.Cities
                .Where(c => c.Id == id)
                .Select(c => new CityDto
                {
                    Id = c.Id,
                    CityName = c.CityName,
                    DistrictsCount = c.Districts.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CityNameExistsAsync(string cityName, int? excludeId = null)
        {
            var query = _context.Cities
                .Where(c => c.CityName.ToLower() == cityName.ToLower());

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<int> GetDistrictsCountAsync(int cityId)
        {
            return await _context.Districts
                .CountAsync(d => d.CityId == cityId);
        }
    }
}