using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.District;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsStore.Infrastructure.Repositories
{
    public class DistrictRepository : BaseRepository<District>, IDistrictRepository
    {
        public DistrictRepository(AppDbContext context) : base(context) { }

        public new async Task<IEnumerable<DistrictDto>> GetAllAsync()
        {
            return await _context.Districts
                .Select(d => new DistrictDto
                {
                    Id = d.Id,
                    DistrictName = d.DistrictName,
                    CityId = d.CityId,
                    CityName = d.City.CityName,
                    AddressesCount = d.Addresses.Count
                })
                .OrderBy(d => d.CityName)
                .ThenBy(d => d.DistrictName)
                .ToListAsync();
        }

       
        public async Task<IEnumerable<DistrictDto>> GetByCityIdAsync(int cityId)
        {
            return await _context.Districts
                .Where(d => d.CityId == cityId)
                .Select(d => new DistrictDto
                {
                    Id = d.Id,
                    DistrictName = d.DistrictName,
                    CityId = d.CityId,
                    CityName = d.City.CityName,
                    AddressesCount = d.Addresses.Count
                })
                .OrderBy(d => d.DistrictName)
                .ToListAsync();
        }

        public async Task<DistrictDto?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Districts
                .Where(d => d.Id == id)
                .Select(d => new DistrictDto
                {
                    Id = d.Id,
                    DistrictName = d.DistrictName,
                    CityId = d.CityId,
                    CityName = d.City.CityName,
                    AddressesCount = d.Addresses.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> DistrictExistsAsync(int cityId, string districtName, int? excludeId = null)
        {
            var query = _context.Districts
                .Where(d => d.CityId == cityId &&
                           d.DistrictName.ToLower() == districtName.ToLower());

            if (excludeId.HasValue)
                query = query.Where(d => d.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<int> GetAddressesCountAsync(int districtId)
        {
            return await _context.Addresses
                .CountAsync(a => a.DistrictId == districtId);
        }
    }
}