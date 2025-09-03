namespace AutoPartsStore.Core.Entities
{
    public class CartItem 
    {
        public int Id { get; private set; }
        public int CartId { get; private set; }
        public int PartId { get; private set; }
        public int Quantity { get; private set; }
        public DateTime AddedDate { get; private set; }

        // Navigation
        public ShoppingCart Cart { get; private set; }
        public CarPart CarPart { get; private set; }

        public CartItem(int cartId, int partId, int quantity = 1)
        {
            CartId = cartId;
            PartId = partId;
            Quantity = quantity;
            AddedDate = DateTime.UtcNow;
            Validate();
        }

        private void Validate()
        {
            if (Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");
        }

        public void UpdateQuantity(int newQuantity)
        {
            Quantity = newQuantity;
            Validate();
        }
    }
}
