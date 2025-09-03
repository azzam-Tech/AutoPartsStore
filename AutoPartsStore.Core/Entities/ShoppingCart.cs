namespace AutoPartsStore.Core.Entities
{
    public class ShoppingCart 
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime LastUpdated { get; private set; }

        // Navigation
        public User User { get; private set; }
        public List<CartItem> Items { get; private set; } = new();

        public ShoppingCart(int userId)
        {
            UserId = userId;
            CreatedDate = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;
        }

        public void UpdateLastUpdated()
        {
            LastUpdated = DateTime.UtcNow;
        }
    }
}
