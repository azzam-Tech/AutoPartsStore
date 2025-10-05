using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models;
using AutoPartsStore.Core.Models.CarPart;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsStore.Infrastructure.Repositories
{
    public class CarPartRepository : BaseRepository<CarPart>, ICarPartRepository
    {
        public CarPartRepository(AppDbContext context) : base(context) { }

        public async Task<CarPartDto> GetByIdWithDetailsAsync(int id)
        {
            return await _context.CarParts
                .Where(p => p.Id == id && !p.IsDeleted)
                .Select(p => new CarPartDto
                {
                    Id = p.Id,
                    PartNumber = p.PartNumber,
                    PartName = p.PartName,
                    Description = p.Description,
                    CarBrand = p.CarBrand,
                    CarModel = p.CarModel,
                    CarYear = p.CarYear,
                    UnitPrice = p.UnitPrice,
                    DiscountPercent = p.DiscountPercent,
                    FinalPrice = p.GetFinalPrice(),
                    StockQuantity = p.StockQuantity,
                    IsActive = p.IsActive,
                    IsInStock = p.IsInStock(),
                    IsOnSale = p.IsOnSale(),
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.CategoryName,
                    AverageRating = p.Reviews
                        .Where(r => r.IsApproved == true) // Only approved reviews
                        .Average(r => (double?)r.Rating) ?? 0,
                    ReviewCount = p.Reviews.Count(r => r.IsApproved == true), // Only approved reviews
                    //TotalSold = p.CartItems
                    //    .Where(ci => ci.Cart.Order != null && ci.Cart.Order.OrderStatus == "Delivered")
                    //    .Sum(ci => ci.Quantity)
                })
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Car part not found.");
        }

        public async Task<PagedResult<CarPartDto>> GetFilteredAsync(CarPartFilter filter)
        {
            var query = _context.CarParts
                .Where(p => !p.IsDeleted)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(p =>
                    p.PartNumber.Contains(filter.SearchTerm) ||
                    p.PartName.Contains(filter.SearchTerm) ||
                   (p.Description != null && p.Description.Contains(filter.SearchTerm)) ||
                   (p.CarBrand != null && p.CarBrand.Contains(filter.SearchTerm)) ||
                   (p.CarModel != null && p.CarModel.Contains(filter.SearchTerm)));
            }

            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId);

            if (!string.IsNullOrEmpty(filter.CarBrand))
                query = query.Where(p => p.CarBrand == filter.CarBrand);

            if (!string.IsNullOrEmpty(filter.CarModel))
                query = query.Where(p => p.CarModel == filter.CarModel);

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.UnitPrice >= filter.MinPrice);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.UnitPrice <= filter.MaxPrice);

            if (filter.InStock.HasValue)
                query = query.Where(p => p.IsInStock() == filter.InStock);

            if (filter.OnSale.HasValue)
                query = query.Where(p => p.IsOnSale() == filter.OnSale);

            if (filter.IsActive.HasValue)
                query = query.Where(p => p.IsActive == filter.IsActive);

            if (filter.SortBy.HasValue)
            {
                // Apply sorting
                query = filter.SortBy switch
                {
                    SortBy.price => filter.SortDescending != null && filter.SortDescending.Value ? query.OrderByDescending(p => p.UnitPrice) : query.OrderBy(p => p.UnitPrice),
                    SortBy.name => filter.SortDescending != null && filter.SortDescending.Value ? query.OrderByDescending(p => p.PartName) : query.OrderBy(p => p.PartName),
                    SortBy.newest => query.OrderByDescending(p => p.CreatedAt),
                    SortBy.rating => query.OrderByDescending(p => p.Reviews
                               .Where(r => r.IsApproved == true) // Only approved reviews
                               .Average(r => (double?)r.Rating) ?? 0),
                    _ => query.OrderBy(p => p.PartName)
                };
            }


            // Apply pagination
            query = query.Skip((filter.Page - 1) * filter.PageSize)
                       .Take(filter.PageSize);

            var result = await query.Select(p => new CarPartDto
            {
                Id = p.Id,
                PartNumber = p.PartNumber,
                PartName = p.PartName,
                Description = p.Description,
                CarBrand = p.CarBrand,
                CarModel = p.CarModel,
                CarYear = p.CarYear,
                UnitPrice = p.UnitPrice,
                DiscountPercent = p.DiscountPercent,
                FinalPrice = p.GetFinalPrice(),
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive,
                IsInStock = p.IsInStock(),
                IsOnSale = p.IsOnSale(),
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.CategoryName,
                AverageRating = p.Reviews
                                    .Where(r => r.IsApproved == true) // Only approved reviews
                                    .Average(r => (double?)r.Rating) ?? 0,
                ReviewCount = p.Reviews.Count(r => r.IsApproved == true) // Only approved reviews
            }).ToListAsync();

            var count = await GetTotalCountAsync(filter);

            PagedResult<CarPartDto> pagedResult = new()
            {
                Items = result,
                CurrentPage = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = count,
                TotalPages = (int)Math.Ceiling((double)count / filter.PageSize)

            };

            return pagedResult;
        }

        public async Task<IEnumerable<CarPartDto>> GetByCategoryAsync(int categoryId)
        {
            return await _context.CarParts
                .Where(p => p.CategoryId == categoryId && !p.IsDeleted && p.IsActive)
                .Select(p => new CarPartDto
                {
                    Id = p.Id,
                    PartNumber = p.PartNumber,
                    PartName = p.PartName,
                    Description = p.Description,
                    CarBrand = p.CarBrand,
                    CarModel = p.CarModel,
                    CarYear = p.CarYear,
                    UnitPrice = p.UnitPrice,
                    DiscountPercent = p.DiscountPercent,
                    FinalPrice = p.GetFinalPrice(),
                    StockQuantity = p.StockQuantity,
                    IsActive = p.IsActive,
                    IsInStock = p.IsInStock(),
                    IsOnSale = p.IsOnSale(),
                    ImageUrl = p.ImageUrl,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.CategoryName
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<CarPartDto>> GetFeaturedAsync(int count)
        {
            return await _context.CarParts
                .Where(p => !p.IsDeleted && p.IsActive && p.StockQuantity > 0)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .Select(p => new CarPartDto
                {
                    Id = p.Id,
                    PartNumber = p.PartNumber,
                    PartName = p.PartName,
                    Description = p.Description,
                    CarBrand = p.CarBrand,
                    CarModel = p.CarModel,
                    CarYear = p.CarYear,
                    UnitPrice = p.UnitPrice,
                    DiscountPercent = p.DiscountPercent,
                    FinalPrice = p.GetFinalPrice(),
                    StockQuantity = p.StockQuantity,
                    IsActive = p.IsActive,
                    IsInStock = p.IsInStock(),
                    IsOnSale = p.IsOnSale(),
                    ImageUrl = p.ImageUrl,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.CategoryName,
                    AverageRating = p.Reviews
                        .Where(r => r.IsApproved == true) // Only approved reviews
                        .Average(r => (double?)r.Rating) ?? 0
                })
                .ToListAsync();
        }

        public async Task<bool> PartNumberExistsAsync(string partNumber, int? excludeId = null)
        {
            var query = _context.CarParts
                .Where(p => p.PartNumber.ToLower() == partNumber.ToLower() && !p.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<int> GetTotalCountAsync(CarPartFilter filter)
        {
            var query = _context.CarParts
                .Where(p => !p.IsDeleted && p.IsActive);

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(p =>
                    p.PartNumber.Contains(filter.SearchTerm) ||
                    p.PartName.Contains(filter.SearchTerm) ||
                   (p.Description != null && p.Description.Contains(filter.SearchTerm)) ||
                   (p.CarBrand != null && p.CarBrand.Contains(filter.SearchTerm)) ||
                   (p.CarModel != null && p.CarModel.Contains(filter.SearchTerm)));
            }

            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId);

            if (!string.IsNullOrEmpty(filter.CarBrand))
                query = query.Where(p => p.CarBrand == filter.CarBrand);

            if (!string.IsNullOrEmpty(filter.CarModel))
                query = query.Where(p => p.CarModel == filter.CarModel);

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.UnitPrice >= filter.MinPrice);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.UnitPrice <= filter.MaxPrice);

            if (filter.InStock.HasValue)
                query = query.Where(p => p.IsInStock() == filter.InStock);

            if (filter.OnSale.HasValue)
                query = query.Where(p => p.IsOnSale() == filter.OnSale);

            if (filter.IsActive.HasValue)
                query = query.Where(p => p.IsActive == filter.IsActive);

            return await query.CountAsync();
        }

        public async Task<decimal> GetMaxPriceAsync()
        {
            return await _context.CarParts
                .Where(p => !p.IsDeleted && p.IsActive)
                .MaxAsync(p => p.UnitPrice);
        }

        public async Task<List<CarPart>> GetPartsByPromotionIdAsync(int promotionId)
        {
            return await _context.CarParts
                .Where(cp => cp.PromotionId == promotionId)
                .ToListAsync();
        }
    }
}