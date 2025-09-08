using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.District
{
    public class UpdateDistrictRequest
    {
        [Required]
        public int CityId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string DistrictName { get; set; }
    }
}