using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.Review
{
    public class ReviewApprovalRequest
    {
        /// <summary>
        /// Approval status: true = Approved, false = Rejected, null = Pending
        /// </summary>
        public bool? IsApproved { get; set; }
    }
}