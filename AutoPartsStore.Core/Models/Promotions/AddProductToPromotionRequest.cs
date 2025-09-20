using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.Promotions
{
    public class AddProductToPromotionRequest
    {
        [Required]
        public int PartId { get; set; }
    }

}