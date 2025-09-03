namespace AutoPartsStore.Core.Entities
{
    public class Address 
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public int DistrictId { get; private set; }
        public string? StreetName { get; private set; }
        public string? StreetNumber { get; private set; }
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
    }
}
