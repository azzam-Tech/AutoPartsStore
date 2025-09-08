using System.ComponentModel.DataAnnotations;

namespace AutoPartsStore.Core.Entities
{
    public class Address
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public int DistrictId { get; private set; }

        [MaxLength(150)]
        public string? StreetName { get; private set; }

        [MaxLength(20)]
        public string? StreetNumber { get; private set; }

        [MaxLength(10)]
        public string? PostalCode { get; private set; }

        // Navigation
        public User User { get; private set; }
        public District District { get; private set; }

        public Address(int userId, int districtId, string? streetName, string? streetNumber, string? postalCode)
        {
            UserId = userId;
            DistrictId = districtId;
            StreetName = streetName;
            StreetNumber = streetNumber;
            PostalCode = postalCode;
        }

        // Methods
        public void UpdateAddress(string? streetName, string? streetNumber, string? postalCode, int districtId)
        {
            StreetName = streetName;
            StreetNumber = streetNumber;
            PostalCode = postalCode;
            DistrictId = districtId;
        }

        public void ChangeDistrict(int districtId)
        {
            DistrictId = districtId;
        }
    }
}