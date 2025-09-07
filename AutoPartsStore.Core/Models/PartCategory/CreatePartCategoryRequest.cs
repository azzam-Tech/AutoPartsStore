using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.PartCategory
{
    public class CreatePartCategoryRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CategoryName { get; set; }

        public int? ParentCategoryId { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Url]
        [StringLength(255)]
        public string ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;
    }
}