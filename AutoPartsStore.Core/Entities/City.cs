namespace AutoPartsStore.Core.Entities
{
    public class City 
    {
        public int Id { get; private set; }
        public string CityName { get; private set; }

        // Relationship
        public List<District> Districts { get; private set; } = new();

        public City(string cityName)
        {
            CityName = cityName;
        }
    }
}
