namespace AutoPartsStore.Core.Entities
{
    public class District
    {
        public int Id { get; private set; }
        public int CityId { get; private set; }
        public string DistrictName { get; private set; }

        // Navigation
        public City City { get; private set; }
        public List<Address> Addresses { get; private set; } = new();

        public District(int cityId, string districtName)
        {
            CityId = cityId;
            DistrictName = districtName;
        }

        // Methods
        public void UpdateName(string districtName)
        {
            if (string.IsNullOrWhiteSpace(districtName))
                throw new ArgumentException("District name cannot be empty");

            DistrictName = districtName;
        }

        public void ChangeCity(int cityId)
        {
            CityId = cityId;
        }
    }
}
