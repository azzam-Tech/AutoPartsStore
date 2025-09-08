using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.Address;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsStore.Infrastructure.Repositories
{
    public class AddressRepository : BaseRepository<Address>, IAddressRepository
    {
        public AddressRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<AddressDto>> GetByUserIdAsync(int userId)
        {
            return await _context.Addresses
                .Where(a => a.UserId == userId)
                .Include(a => a.User)
                .Include(a => a.District)
                .ThenInclude(d => d.City)
                .Select(a => new AddressDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User.FullName,
                    DistrictId = a.DistrictId,
                    DistrictName = a.District.DistrictName,
                    CityId = a.District.City.Id,
                    CityName = a.District.City.CityName,
                    StreetName = a.StreetName,
                    StreetNumber = a.StreetNumber,
                    PostalCode = a.PostalCode,
                    FullAddress = $"{a.StreetName} {a.StreetNumber}, {a.District.DistrictName}, {a.District.City.CityName}, {a.PostalCode}"
                })
                .OrderBy(a => a.CityName)
                .ThenBy(a => a.DistrictName)
                .ToListAsync();
        }

        public async Task<AddressDto> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Addresses
                .Where(a => a.Id == id)
                .Include(a => a.User)
                .Include(a => a.District)
                .ThenInclude(d => d.City)
                .Select(a => new AddressDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User.FullName,
                    DistrictId = a.DistrictId,
                    DistrictName = a.District.DistrictName,
                    CityId = a.District.City.Id,
                    CityName = a.District.City.CityName,
                    StreetName = a.StreetName,
                    StreetNumber = a.StreetNumber,
                    PostalCode = a.PostalCode,
                    FullAddress = $"{a.StreetName} {a.StreetNumber}, {a.District.DistrictName}, {a.District.City.CityName}, {a.PostalCode}"
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UserHasAddressAsync(int userId, int districtId, string streetName, string streetNumber, int? excludeId = null)
        {
            var query = _context.Addresses
                .Where(a => a.UserId == userId &&
                           a.DistrictId == districtId &&
                           a.StreetName == streetName &&
                           a.StreetNumber == streetNumber);

            if (excludeId.HasValue)
                query = query.Where(a => a.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}