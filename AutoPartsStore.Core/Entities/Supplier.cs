namespace AutoPartsStore.Core.Entities
{
    public class Supplier 
    {
        public int Id { get; private set; }
        public string SupplierName { get; private set; }
        public string? ContactPerson { get; private set; }
        public string? Email { get; private set; }
        public string PhoneNumber { get; private set; }
        public string? Address { get; private set; }
        public bool IsActive { get; private set; }

        // Relationship
        public List<PartSupply> Supplies { get; private set; } = new();

        public Supplier(string supplierName, string phoneNumber, string? contactPerson = null, string? email = null, string? address = null)
        {
            SupplierName = supplierName;
            ContactPerson = contactPerson;
            Email = email;
            PhoneNumber = phoneNumber;
            Address = address;
            IsActive = true;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}
