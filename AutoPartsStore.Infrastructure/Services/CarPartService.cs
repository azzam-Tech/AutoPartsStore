using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.CarPart;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoPartsStore.Infrastructure.Services
{
    public class CarPartService : ICarPartService
    {
        private readonly ICarPartRepository _partRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<CarPartService> _logger;

        public CarPartService(ICarPartRepository partRepository, AppDbContext context, ILogger<CarPartService> logger)
        {
            _partRepository = partRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<CarPartDto> GetByIdAsync(int id)
        {
            return await _partRepository.GetByIdWithDetailsAsync(id);
        }

        public async Task<IEnumerable<CarPartDto>> GetFilteredAsync(CarPartFilter filter)
        {
            return await _partRepository.GetFilteredAsync(filter);
        }

        public async Task<IEnumerable<CarPartDto>> GetFeaturedAsync(int count = 8)
        {
            return await _partRepository.GetFeaturedAsync(count);
        }

        public async Task<CarPartDto> CreateAsync(CreateCarPartRequest request)
        {
            if (await _partRepository.PartNumberExistsAsync(request.PartNumber))
                throw new InvalidOperationException($"Part number '{request.PartNumber}' already exists.");

            var category = await _context.PartCategories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDeleted);

            if (category == null)
                throw new InvalidOperationException("Category not found.");

            var part = new CarPart(
                request.PartNumber,
                request.CategoryId,
                request.PartName,
                request.UnitPrice,
                request.StockQuantity,
                request.Description,
                request.CarBrand,
                request.CarModel,
                request.CarYear,
                request.DiscountPercent,
                request.ImageUrl
            );
            if (request.IsActive)
                part.Activate();
            else
                part.Deactivate();

            _context.CarParts.Add(part);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Car part created: {PartNumber}", request.PartNumber);
            return await _partRepository.GetByIdWithDetailsAsync(part.Id);
        }

        public async Task<CarPartDto> UpdateAsync(int id, UpdateCarPartRequest request)
        {
            var part = await _context.CarParts.FindAsync(id);
            if (part == null || part.IsDeleted)
                throw new KeyNotFoundException("Car part not found.");

            part.UpdateBasicInfo(
                request.PartName,
                request.Description,
                request.CarBrand,
                request.CarModel,
                request.CarYear
            );

            part.UpdatePrice(request.UnitPrice);
            part.UpdateDiscount(request.DiscountPercent);
            part.UpdateStock(request.StockQuantity);
            part.UpdateImage(request.ImageUrl);
            if (request.IsActive)
                part.Activate();
            else
                part.Deactivate();

            await _context.SaveChangesAsync();

            _logger.LogInformation("Car part updated: {PartNumber}", part.PartNumber);
            return await _partRepository.GetByIdWithDetailsAsync(part.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var part = await _context.CarParts.FindAsync(id);
            if (part == null || part.IsDeleted)
                throw new KeyNotFoundException("Car part not found.");

            part.SoftDelete();
            await _context.SaveChangesAsync();

            _logger.LogInformation("Car part deleted: {PartNumber}", part.PartNumber);
            return true;
        }

        public async Task<bool> UpdateStockAsync(int id, int quantity)
        {
            var part = await _context.CarParts.FindAsync(id);
            if (part == null || part.IsDeleted)
                throw new KeyNotFoundException("Car part not found.");

            part.UpdateStock(quantity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Stock updated for part {PartNumber}: {Quantity}", part.PartNumber, quantity);
            return true;
        }

        public async Task<bool> UpdatePriceAsync(int id, decimal price)
        {
            var part = await _context.CarParts.FindAsync(id);
            if (part == null || part.IsDeleted)
                throw new KeyNotFoundException("Car part not found.");

            part.UpdatePrice(price);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Price updated for part {PartNumber}: {Price}", part.PartNumber, price);
            return true;
        }

        public async Task<decimal> GetMaxPriceAsync()
        {
            return await _partRepository.GetMaxPriceAsync();
        }
    }
}