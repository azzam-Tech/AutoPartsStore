using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.Review
{
    public class ProductReviewDto
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public string PartName { get; set; }
        public string PartNumber { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; }
        public DateTime ReviewDate { get; set; }
        public bool IsApproved { get; set; }
        public string Status { get; set; }
    }

    public class ReviewVoteRequest
    {
        [Required]
        public bool IsHelpful { get; set; }
    }

    public class ReviewVoteDto
    {
        public int ReviewId { get; set; }
        public int HelpfulVotes { get; set; }
        public int TotalVotes { get; set; }
        public decimal HelpfulPercentage { get; set; }
        public bool? UserVote { get; set; }
    }
}