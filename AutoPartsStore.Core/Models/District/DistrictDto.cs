namespace AutoPartsStore.Core.Models.District
{
    public class DistrictDto
    {
        public int Id { get; set; }
        public string DistrictName { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int AddressesCount { get; set; }
    }
}