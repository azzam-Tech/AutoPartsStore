using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.Promotion
{
    public class AddProductToPromotionRequest
    {
        [Required]
        public int PartId { get; set; }
    }
}