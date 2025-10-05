using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.Review
{
    public class ProductReviewDto
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public string PartName { get; set; } = null!;
        public string PartNumber { get; set; } = null!;
        public int? UserId { get; set; }
        public string UserName { get; set; } = null!;
        public int Rating { get; set; }
        public string? ReviewText { get; set; }
        public DateTime ReviewDate { get; set; }
        public bool? IsApproved { get; set; } // null = Pending, true = Approved, false = Rejected
        public string Status { get; set; } = null!;
    }
}