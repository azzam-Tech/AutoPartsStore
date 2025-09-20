namespace AutoPartsStore.Core.Models.Address
{
    public class AddressDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int DistrictId { get; set; }
        public string? DistrictName { get; set; }
        public int CityId { get; set; }
        public string? CityName { get; set; }
        public string? StreetName { get; set; }
        public string? StreetNumber { get; set; }
        public string? PostalCode { get; set; }
        public string? FullAddress { get; set; }
    }
}