using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.Review
{
    public class ReviewApprovalRequest
    {
        [Required]
        public bool IsApproved { get; set; }
    }
}