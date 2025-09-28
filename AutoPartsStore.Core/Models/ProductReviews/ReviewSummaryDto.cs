namespace AutoPartsStore.Core.Models.Review
{
    public class ReviewSummaryDto
    {
        public int PartId { get; set; }
        public string PartName { get; set; } = null!;
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStar { get; set; }
        public int FourStar { get; set; }
        public int ThreeStar { get; set; }
        public int TwoStar { get; set; }
        public int OneStar { get; set; }
        public List<ProductReviewDto> RecentReviews { get; set; } = new();
    }
}