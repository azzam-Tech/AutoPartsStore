namespace AutoPartsStore.Core.Entities
{
    public class InventoryLog 
    {
        public int Id { get; private set; }
        public int PartId { get; private set; }
        public char ChangeType { get; private set; } // I=Increase, O=Decrease, A=Adjustment, D=Damage
        public int Quantity { get; private set; }
        public int PreviousQuantity { get; private set; }
        public int NewQuantity { get; private set; }
        public DateTime ChangeDate { get; private set; }
        public int? ChangedByUserId { get; private set; }
        public string? Notes { get; private set; }

        // Navigation
        public CarPart CarPart { get; private set; }
        public User? ChangedByUser { get; private set; }

        public InventoryLog(
            int partId, char changeType, int quantity,
            int previousQuantity, int newQuantity,
            int? changedByUserId = null, string? notes = null)
        {
            PartId = partId;
            ChangeType = changeType;
            Quantity = quantity;
            PreviousQuantity = previousQuantity;
            NewQuantity = newQuantity;
            ChangedByUserId = changedByUserId;
            Notes = notes;
            ChangeDate = DateTime.UtcNow;

            Validate();
        }

        private void Validate()
        {
            if (!"IOAD".Contains(ChangeType))
                throw new ArgumentException("ChangeType must be I, O, A, or D");
            if (Quantity <= 0)
                throw new ArgumentException("Quantity must be positive");
        }
    }
}
