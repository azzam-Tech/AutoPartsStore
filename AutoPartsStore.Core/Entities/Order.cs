namespace AutoPartsStore.Core.Entities
{
    /// <summary>
    /// Order status enum
    /// </summary>
    public enum OrderStatus
    {
        Pending = 0,           // Order created, awaiting payment
        PaymentPending = 1,    // Payment initiated
        Paid = 2,              // Payment successful
        Processing = 3,        // Order is being prepared
        Shipped = 4,           // Order has been shipped
        Delivered = 5,         // Order delivered to customer
        Cancelled = 6,         // Order cancelled
        Refunded = 7,          // Payment refunded
        Failed = 8             // Payment or order processing failed
    }

    /// <summary>
    /// Main Order entity - Simplified without tax and shipping
    /// Uses unified pricing logic: Totals = Sum of OrderItems
    /// </summary>
    public class Order
    {
        public int Id { get; private set; }
        public string OrderNumber { get; private set; } // Unique order reference
        public int UserId { get; private set; }
        
        // Address information
        public int ShippingAddressId { get; private set; }
        
        // Order amounts - PUBLIC for external calculations
        // These should always equal the sum of OrderItems amounts
        public decimal SubTotal { get; set; }        // Sum of OrderItems.SubTotal (TotalPrice)
        public decimal DiscountAmount { get; set; }  // Sum of OrderItems.DiscountAmount (TotalDiscount)
        public decimal TotalAmount { get; set; }     // SubTotal - DiscountAmount (FinalTotal)
        
        // Order status and tracking
        public OrderStatus Status { get; private set; }
        public DateTime OrderDate { get; private set; }
        public DateTime? PaidDate { get; private set; }
        public DateTime? ShippedDate { get; private set; }
        public DateTime? DeliveredDate { get; private set; }
        public DateTime? CancelledDate { get; private set; }
        public string? CancellationReason { get; private set; }
        
        // Payment information
        public int? PaymentTransactionId { get; private set; }
        
        // Additional information
        public string? CustomerNotes { get; private set; }
        public string? AdminNotes { get; private set; }
        public string? TrackingNumber { get; private set; }
        
        // Audit
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        
        // Navigation properties
        public User User { get; private set; } = null!;
        public Address ShippingAddress { get; private set; } = null!;
        public PaymentTransaction? PaymentTransaction { get; private set; }
        public List<OrderItem> OrderItems { get; private set; } = new();

        // Constructor - Simplified
        public Order(
            int userId,
            int shippingAddressId,
            decimal subTotal,
            decimal discountAmount,
            string? customerNotes = null)
        {
            UserId = userId;
            ShippingAddressId = shippingAddressId;
            SubTotal = subTotal;
            DiscountAmount = discountAmount;
            TotalAmount = subTotal - discountAmount;
            
            OrderNumber = GenerateOrderNumber();
            Status = OrderStatus.Pending;
            OrderDate = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
            CustomerNotes = customerNotes;
            IsDeleted = false;

            Validate();
        }

        private void Validate()
        {
            if (SubTotal < 0)
                throw new ArgumentException("SubTotal cannot be negative");
            if (DiscountAmount < 0)
                throw new ArgumentException("DiscountAmount cannot be negative");
            if (DiscountAmount > SubTotal)
                throw new ArgumentException("DiscountAmount cannot exceed SubTotal");
            if (TotalAmount < 0)
                throw new ArgumentException("TotalAmount cannot be negative");
        }

        // Methods
        public void UpdateStatus(OrderStatus newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;

            switch (newStatus)
            {
                case OrderStatus.Paid:
                    PaidDate = DateTime.UtcNow;
                    break;
                case OrderStatus.Shipped:
                    ShippedDate = DateTime.UtcNow;
                    break;
                case OrderStatus.Delivered:
                    DeliveredDate = DateTime.UtcNow;
                    break;
                case OrderStatus.Cancelled:
                    CancelledDate = DateTime.UtcNow;
                    break;
            }
        }

        public void Cancel(string reason)
        {
            if (Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Cannot cancel delivered or already cancelled order");

            Status = OrderStatus.Cancelled;
            CancelledDate = DateTime.UtcNow;
            CancellationReason = reason;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AssignPaymentTransaction(int paymentTransactionId)
        {
            PaymentTransactionId = paymentTransactionId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateTrackingNumber(string trackingNumber)
        {
            TrackingNumber = trackingNumber;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddAdminNotes(string notes)
        {
            AdminNotes = notes;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SoftDelete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Restore()
        {
            IsDeleted = false;
            DeletedAt = null;
            UpdatedAt = DateTime.UtcNow;
        }

        private static string GenerateOrderNumber()
        {
            // Format: ORD-YYYYMMDD-XXXXX (e.g., ORD-20250105-12345)
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = new Random().Next(10000, 99999);
            return $"ORD-{datePart}-{randomPart}";
        }

        public bool CanBeCancelled()
        {
            return Status != OrderStatus.Delivered && 
                   Status != OrderStatus.Cancelled && 
                   Status != OrderStatus.Refunded;
        }

        public bool CanBeRefunded()
        {
            return Status == OrderStatus.Paid || 
                   Status == OrderStatus.Processing || 
                   Status == OrderStatus.Shipped;
        }

        /// <summary>
        /// Recalculate order totals from order items
        /// This should be called after OrderItems are created or modified
        /// Formula: SubTotal - DiscountAmount = TotalAmount
        /// </summary>
        public void UpdateTotals(decimal subTotal, decimal discountAmount)
        {
            SubTotal = subTotal;
            DiscountAmount = discountAmount;
            TotalAmount = subTotal - discountAmount;
            UpdatedAt = DateTime.UtcNow;
            
            Validate();
        }

        /// <summary>
        /// Recalculate order totals from OrderItems collection
        /// Ensures Order totals always match sum of OrderItems
        /// </summary>
        public void RecalculateTotalsFromItems()
        {
            if (OrderItems == null || !OrderItems.Any())
            {
                SubTotal = 0;
                DiscountAmount = 0;
                TotalAmount = 0;
            }
            else
            {
                SubTotal = OrderItems.Sum(oi => oi.SubTotal);
                DiscountAmount = OrderItems.Sum(oi => oi.DiscountAmount);
                TotalAmount = SubTotal - DiscountAmount;
            }
            
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
