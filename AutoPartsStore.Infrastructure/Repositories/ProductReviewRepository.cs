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

        public async Task<List<ProductReviewDto>> GetReviewsByPartIdAsync(int partId, bool? approvedOnly = true)
        {
            var query = _context.ProductReviews
                .Where(r => r.PartId == partId)
                .Include(r => r.CarPart)
                .Include(r => r.User)
                .AsQueryable();

            if (approvedOnly.HasValue)
                query = query.Where(r => r.IsApproved == approvedOnly.Value);

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
                    Status = r.IsApproved ? "Approved" : "Pending",
                })
                .ToListAsync();
        }

        public async Task<List<ProductReviewDto>> GetReviewsByUserIdAsync(int userId)
        {
            return await _context.ProductReviews
                .Where(r => r.UserId == userId)
                .Include(r => r.CarPart)
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
                    Status = r.IsApproved ? "Approved" : "Pending"
                })
                .ToListAsync();
        }

        public async Task<List<ProductReviewDto>> GetPendingReviewsAsync()
        {
            return await _context.ProductReviews
                .Where(r => !r.IsApproved)
                .Include(r => r.CarPart)
                .Include(r => r.User)
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
                .Include(r => r.CarPart)
                .Include(r => r.User)
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
                    Status = r.IsApproved ? "Approved" : "Pending",
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ReviewSummaryDto> GetReviewSummaryAsync(int partId)
        {
            var reviews = await _context.ProductReviews
                .Where(r => r.PartId == partId && r.IsApproved)
                .ToListAsync();

            var part = await _context.CarParts
                .FirstOrDefaultAsync(p => p.Id == partId);

            var recentReviews = await _context.ProductReviews
                .Where(r => r.PartId == partId && r.IsApproved)
                .Include(r => r.User)
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
                PartName = part?.PartName,
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
            return await _context.ProductReviews
                .Where(r => r.PartId == partId && r.IsApproved)
                .AverageAsync(r => (double?)r.Rating) ?? 0;
        }

        public async Task<int> GetReviewCountAsync(int partId, bool? approvedOnly = true)
        {
            var query = _context.ProductReviews.Where(r => r.PartId == partId);

            if (approvedOnly.HasValue)
                query = query.Where(r => r.IsApproved == approvedOnly.Value);

            return await query.CountAsync();
        }

        public async Task<bool> HasUserReviewedPartAsync(int userId, int partId)
        {
            return await _context.ProductReviews
                .AnyAsync(r => r.PartId == partId && r.UserId == userId);
        }
    }
}