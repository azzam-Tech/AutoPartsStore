using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.Cart
{
    public class UpdateCartItemRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}