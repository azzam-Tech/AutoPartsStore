using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.Favorites
{
    public class AddToFavoriteRequest
    {
        [Required]
        public int PartId { get; set; }
    }
}
