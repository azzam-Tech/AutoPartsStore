namespace AutoPartsStore.Core.Entities
{
    public class PartSupply 
    {
        public int Id { get; private set; }
        public int PartId { get; private set; }
        public int SupplierId { get; private set; }
        public decimal SupplyPrice { get; private set; }
        public DateTime? LastSupplyDate { get; private set; }

        // Navigation
        public CarPart CarPart { get; private set; }
        public Supplier Supplier { get; private set; }

        public PartSupply(int partId, int supplierId, decimal supplyPrice)
        {
            PartId = partId;
            SupplierId = supplierId;
            SupplyPrice = supplyPrice;
        }

        public void UpdateSupplyPrice(decimal newPrice)
        {
            if (newPrice <= 0) throw new ArgumentException("Supply price must be > 0");
            SupplyPrice = newPrice;
        }

        public void SetLastSupplyDate(DateTime date)
        {
            LastSupplyDate = date;
        }
    }
}
