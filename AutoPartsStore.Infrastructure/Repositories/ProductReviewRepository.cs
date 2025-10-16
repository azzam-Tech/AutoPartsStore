using AutoPartsStore.Core.Entities;
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Models.Review;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsStore.Infrastructure.Repositories
{
    public class ProductReviewRepository : BaseRepository<ProductReview>, IProductReviewRepository
    {
        public ProductReviewRepository(AppDbContext context) : base(context) { }

        public async Task<List<ProductReviewDto>> GetReviewsAsync(ProductReviewstatus? productReviewstatus)
        {
            var query = _context.ProductReviews
                .AsQueryable();

            if (productReviewstatus.HasValue)
            {
                switch (productReviewstatus.Value)
                {
                    case ProductReviewstatus.IsApproved:
                        query = query.Where(r => r.IsApproved == true);
                        break;
                    case ProductReviewstatus.IsNotApproved:
                        query = query.Where(r => r.IsApproved == false);
                        break;
                    case ProductReviewstatus.IsPending:
                        query = query.Where(r => r.IsApproved == null);
                        break;
                }
            }

            return await query
                .OrderByDescending(r => r.ReviewDate)
                .Select(r => new ProductReviewDto
                {
                    Id = r.Id,
                    PartId = r.PartId,
                    PartName = r.CarPart.PartName,
                    PartNumber = r.CarPart.PartNumber,
                    UserId = r.UserId,
                    UserName = r.UserId.HasValue ? r.User.FullName : "Anonymous",
                    Rating = r.Rating,
                    ReviewText = r.ReviewText,
                    ReviewDate = r.ReviewDate,
                    IsApproved = r.IsApproved,
                    Status = r.IsApproved == true ? "Approved" : (r.IsApproved == false ? "Rejected" : "Pending")
                })
                .ToListAsync();
        }

        public async Task<List<ProductReviewDto>> GetReviewsByPartIdAsync(int partId, ProductReviewstatus? productReviewstatus)
        {
            var query = _context.ProductReviews
                .Where(r => r.PartId == partId)
                .AsQueryable();

            if (productReviewstatus.HasValue)
            {
                switch (productReviewstatus.Value)
                {
                    case ProductReviewstatus.IsApproved:
                        query = query.Where(r => r.IsApproved == true);
                        break;
                    case ProductReviewstatus.IsNotApproved:
                        query = query.Where(r => r.IsApproved == false);
                        break;
                    case ProductReviewstatus.IsPending:
                        query = query.Where(r => r.IsApproved == null);
                        break;
                }
            }

            return await query
                .OrderByDescending(r => r.ReviewDate)
                .Select(r => new ProductReviewDto
                {
                    Id = r.Id,
                    PartId = r.PartId,
                    PartName = r.CarPart.PartName,
                    PartNumber = r.CarPart.PartNumber,
                    UserId = r.UserId,
                    UserName = r.UserId.HasValue ? r.User.FullName : "Anonymous",
                    Rating = r.Rating,
                    ReviewText = r.ReviewText,
                    ReviewDate = r.ReviewDate,
                    IsApproved = r.IsApproved,
                    Status = r.IsApproved == true ? "Approved" : (r.IsApproved == false ? "Rejected" : "Pending")
                })
                .ToListAsync();
        }

        public async Task<List<ProductReviewDto>> GetReviewsByUserIdAsync(int userId)
        {
            return await _context.ProductReviews
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReviewDate)
                .Select(r => new ProductReviewDto
                {
                    Id = r.Id,
                    PartId = r.PartId,
                    PartName = r.CarPart.PartName,
                    PartNumber = r.CarPart.PartNumber,
                    UserId = r.UserId,
                    UserName = r.User.FullName,
                    Rating = r.Rating,
                    ReviewText = r.ReviewText,
                    ReviewDate = r.ReviewDate,
                    IsApproved = r.IsApproved,
                    Status = r.IsApproved == true ? "Approved" : (r.IsApproved == false ? "Rejected" : "Pending")
                })
                .ToListAsync();
        }

        public async Task<List<ProductReviewDto>> GetPendingReviewsAsync()
        {
            return await _context.ProductReviews
                .Where(r => r.IsApproved == null) // Only get pending reviews (null)
                .OrderBy(r => r.ReviewDate)
                .Select(r => new ProductReviewDto
                {
                    Id = r.Id,
                    PartId = r.PartId,
                    PartName = r.CarPart.PartName,
                    PartNumber = r.CarPart.PartNumber,
                    UserId = r.UserId,
                    UserName = r.UserId.HasValue ? r.User.FullName : "Anonymous",
                    Rating = r.Rating,
                    ReviewText = r.ReviewText,
                    ReviewDate = r.ReviewDate,
                    IsApproved = r.IsApproved,
                    Status = "Pending"
                })
                .ToListAsync();
        }

        public async Task<ProductReviewDto> GetReviewWithDetailsAsync(int reviewId)
        {
            return await _context.ProductReviews
                .Where(r => r.Id == reviewId)
                .Select(r => new ProductReviewDto
                {
                    Id = r.Id,
                    PartId = r.PartId,
                    PartName = r.CarPart.PartName,
                    PartNumber = r.CarPart.PartNumber,
                    UserId = r.UserId,
                    UserName = r.UserId.HasValue ? r.User.FullName : "Anonymous",
                    Rating = r.Rating,
                    ReviewText = r.ReviewText,
                    ReviewDate = r.ReviewDate,
                    IsApproved = r.IsApproved,
                    Status = r.IsApproved == true ? "Approved" : (r.IsApproved == false ? "Rejected" : "Pending")
                })
                .FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Review not found");
        }

        public async Task<ReviewSummaryDto> GetReviewSummaryAsync(int partId)
        {
            var part = await _context.CarParts
                .FirstOrDefaultAsync(p => p.Id == partId);

            if (part == null)
                throw new KeyNotFoundException("Product not found");

            // Only get approved reviews for summary
            var reviews = await _context.ProductReviews
                .Where(r => r.PartId == partId && r.IsApproved == true)
                .ToListAsync();

            var recentReviews = await _context.ProductReviews
                .Where(r => r.PartId == partId && r.IsApproved == true)
                .OrderByDescending(r => r.ReviewDate)
                .Take(5)
                .Select(r => new ProductReviewDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.UserId.HasValue ? r.User.FullName : "Anonymous",
                    Rating = r.Rating,
                    ReviewText = r.ReviewText,
                    ReviewDate = r.ReviewDate
                })
                .ToListAsync();

            return new ReviewSummaryDto
            {
                PartId = partId,
                PartName = part!.PartName,
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
                TotalReviews = reviews.Count,
                FiveStar = reviews.Count(r => r.Rating == 5),
                FourStar = reviews.Count(r => r.Rating == 4),
                ThreeStar = reviews.Count(r => r.Rating == 3),
                TwoStar = reviews.Count(r => r.Rating == 2),
                OneStar = reviews.Count(r => r.Rating == 1),
                RecentReviews = recentReviews
            };
        }

        public async Task<double> GetAverageRatingAsync(int partId)
        {
            // Only calculate average from approved reviews
            return await _context.ProductReviews
                .Where(r => r.PartId == partId && r.IsApproved == true)
                .AverageAsync(r => (double?)r.Rating) ?? 0;
        }

        public async Task<int> GetReviewCountAsync(int partId, ProductReviewstatus? productReviewstatus)
        {
            var query = _context.ProductReviews.Where(r => r.PartId == partId);

            if (productReviewstatus.HasValue)
            {
                switch (productReviewstatus.Value)
                {
                    case ProductReviewstatus.IsApproved:
                        query = query.Where(r => r.IsApproved == true);
                        break;
                    case ProductReviewstatus.IsNotApproved:
                        query = query.Where(r => r.IsApproved == false);
                        break;
                    case ProductReviewstatus.IsPending:
                        query = query.Where(r => r.IsApproved == null);
                        break;
                }
            }

            return await query.CountAsync();
        }

        public async Task<bool> HasUserReviewedPartAsync(int userId, int partId)
        {
            return await _context.ProductReviews
                .AnyAsync(r => r.PartId == partId && r.UserId == userId);
        }
    }
}