namespace AutoPartsStore.Core.Entities
{
    public class Favorite
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public int PartId { get; private set; }
        public DateTime AddedDate { get; private set; }

        // Navigation
        public User User { get; private set; }
        public CarPart CarPart { get; private set; }

        public Favorite(int userId, int partId)
        {
            UserId = userId;
            PartId = partId;
            AddedDate = DateTime.UtcNow;
        }
    }
}