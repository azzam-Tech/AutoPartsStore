using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.City
{
    public class UpdateCityRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CityName { get; set; }
    }
}