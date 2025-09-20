using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.District
{
    public class CreateDistrictRequest
    {
        [Required]
        public int CityId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string DistrictName { get; set; }
    }
}