using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Models.Address
{
    public class CreateAddressRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int DistrictId { get; set; }

        [StringLength(150)]
        public string StreetName { get; set; }

        [StringLength(20)]
        public string StreetNumber { get; set; }

        [StringLength(10)]
        public string PostalCode { get; set; }
    }
}