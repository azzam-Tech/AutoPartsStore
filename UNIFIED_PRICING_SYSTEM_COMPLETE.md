# ?? UNIFIED PRICING SYSTEM - COMPLETE DOCUMENTATION

## Overview
This document explains the unified pricing system across the entire application: Shopping Cart, Orders, and Order Items.

---

## ?? Core Pricing Concepts

### 1. Price Definitions

| Term | Definition | Formula | Example |
|------|------------|---------|---------|
| **UnitPrice** | Original product price (before any discount) | - | 500 SAR |
| **FinalPrice** | Price per unit after best discount | Calculated below | 425 SAR |
| **TotalPrice (SubTotal)** | Total price before discount | `UnitPrice × Quantity` | 1000 SAR (qty=2) |
| **TotalDiscount** | Total discount amount | `TotalPrice - FinalTotal` | 150 SAR |
| **FinalTotal** | Final total after discount | `FinalPrice × Quantity` | 850 SAR |

### 2. Discount Types

```csharp
public enum DiscountType
{
    Percent = 0,  // Percentage discount (e.g., 15%)
    Fixed = 1     // Fixed amount discount (e.g., 50 SAR)
}
```

---

## ?? THE GOLDEN RULE

### **Product Discount has PRIORITY over Promotion**

```
IF product has DiscountPercent > 0
    THEN use Product Discount
ELSE IF product has active Promotion
    THEN use Promotion
ELSE
    No discount
```

### Why This Rule?

1. **Business Logic**: Product-specific discounts are targeted and should take precedence
2. **Consistency**: Single clear rule across all systems
3. **Transparency**: Customers see which discount they're getting

---

## ?? Implementation Across Systems

### 1. **OrderItem Entity** ?

```csharp
// Pricing properties are PUBLIC for calculations
public decimal UnitPrice { get; set; }
public decimal DiscountPercent { get; set; }
public int Quantity { get; set; }

// Promotion info
public int? PromotionId { get; set; }
public DiscountType? PromotionDiscountType { get; set; }
public decimal? PromotionDiscountValue { get; set; }

// Calculated amounts (PUBLIC)
public decimal SubTotal { get; set; }      // = TotalPrice
public decimal DiscountAmount { get; set; } // = TotalDiscount
public decimal FinalPrice { get; set; }     // Per unit after discount
public decimal TotalAmount { get; set; }    // = FinalTotal
```

**Calculation Logic:**
```csharp
private void CalculateAmounts()
{
    // Step 1: Calculate SubTotal (TotalPrice)
    SubTotal = UnitPrice * Quantity;
    
    // Step 2: Calculate FinalPrice (PRIORITY RULE)
    if (DiscountPercent > 0)
    {
        // Product discount has priority
        FinalPrice = UnitPrice * (1 - DiscountPercent / 100);
    }
    else if (PromotionId.HasValue && PromotionDiscountType.HasValue)
    {
        // Use promotion if no product discount
        if (PromotionDiscountType == DiscountType.Percent)
        {
            FinalPrice = UnitPrice * (1 - PromotionDiscountValue.Value / 100);
        }
        else // Fixed
        {
            FinalPrice = Math.Max(0, UnitPrice - PromotionDiscountValue.Value);
        }
    }
    else
    {
        FinalPrice = UnitPrice;
    }
    
    // Step 3: Calculate TotalAmount (FinalTotal)
    TotalAmount = FinalPrice * Quantity;
    
    // Step 4: Calculate DiscountAmount (TotalDiscount)
    DiscountAmount = SubTotal - TotalAmount;
}
```

### 2. **PricingService** ?

```csharp
// New unified methods
decimal CalculateFinalPrice(decimal unitPrice, decimal productDiscountPercent, Promotion? promotion);
decimal CalculateTotalDiscount(decimal unitPrice, decimal productDiscountPercent, Promotion? promotion, int quantity);
decimal CalculateFinalTotal(decimal unitPrice, decimal productDiscountPercent, Promotion? promotion, int quantity);

// Legacy methods (for backward compatibility)
decimal CalculateFinalPrice(decimal unitPrice, DiscountType discountType, decimal discountValue);
decimal CalculateFinalTotal(decimal unitPrice, DiscountType discountType, decimal discountValue, int quantity);
decimal CalculateTotalDiscount(decimal unitPrice, DiscountType discountType, decimal discountValue, int quantity);
```

**Usage Example:**
```csharp
var finalPrice = _pricingService.CalculateFinalPrice(
    unitPrice: 500,
    productDiscountPercent: 10,
    promotion: activePromotion
);
// Returns 450 (uses product discount, ignores promotion)
```

### 3. **Shopping Cart** ?

**ShoppingCartService:**
```csharp
TotalDiscount = cart.Items.Sum(ci => 
    ci.CarPart.DiscountPercent > 0
        ? (ci.CarPart.UnitPrice * ci.CarPart.DiscountPercent / 100) * ci.Quantity
        : (ci.CarPart.Promotion != null && ci.CarPart.Promotion.IsActiveNow()
            ? _pricingService.CalculateTotalDiscount(ci.CarPart.UnitPrice, 
                ci.CarPart.Promotion.DiscountType, 
                ci.CarPart.Promotion.DiscountValue, 
                ci.Quantity)
            : 0)
);
```

### 4. **Order Entity** ?

```csharp
public class Order
{
    public decimal SubTotal { get; set; }        // Sum of OrderItems.SubTotal
    public decimal DiscountAmount { get; set; }  // Sum of OrderItems.DiscountAmount
    public decimal TotalAmount { get; set; }     // SubTotal - DiscountAmount
}

public void UpdateTotals(decimal subTotal, decimal discountAmount)
{
    SubTotal = subTotal;
    DiscountAmount = discountAmount;
    TotalAmount = subTotal - discountAmount;
}
```

**OrderService:**
```csharp
// Recalculate from OrderItems
var orderItems = await _context.OrderItems
    .Where(oi => oi.OrderId == order.Id)
    .ToListAsync();

decimal actualSubTotal = orderItems.Sum(oi => oi.SubTotal);
decimal actualDiscount = orderItems.Sum(oi => oi.DiscountAmount);

order.UpdateTotals(actualSubTotal, actualDiscount);
```

---

## ?? Complete Examples

### Example 1: Product Discount Only

```
Product: Brake Pad
UnitPrice: 500 SAR
DiscountPercent: 10%
Promotion: None
Quantity: 2

Calculations:
- SubTotal (TotalPrice) = 500 × 2 = 1000 SAR
- FinalPrice = 500 × (1 - 10/100) = 450 SAR
- TotalAmount (FinalTotal) = 450 × 2 = 900 SAR
- DiscountAmount (TotalDiscount) = 1000 - 900 = 100 SAR
```

### Example 2: Promotion Only

```
Product: Oil Filter
UnitPrice: 300 SAR
DiscountPercent: 0%
Promotion: 15% off
Quantity: 3

Calculations:
- SubTotal (TotalPrice) = 300 × 3 = 900 SAR
- FinalPrice = 300 × (1 - 15/100) = 255 SAR
- TotalAmount (FinalTotal) = 255 × 3 = 765 SAR
- DiscountAmount (TotalDiscount) = 900 - 765 = 135 SAR
```

### Example 3: Both Discounts (Product Wins!)

```
Product: Spark Plug
UnitPrice: 200 SAR
DiscountPercent: 20%        ? WINNER (has priority)
Promotion: 25% off          ? IGNORED
Quantity: 4

Calculations:
- SubTotal (TotalPrice) = 200 × 4 = 800 SAR
- FinalPrice = 200 × (1 - 20/100) = 160 SAR  ? Product discount used
- TotalAmount (FinalTotal) = 160 × 4 = 640 SAR
- DiscountAmount (TotalDiscount) = 800 - 640 = 160 SAR

Note: Promotion 25% is NOT used because product has its own discount!
```

### Example 4: Fixed Amount Promotion

```
Product: Air Filter
UnitPrice: 150 SAR
DiscountPercent: 0%
Promotion: 30 SAR off (Fixed)
Quantity: 2

Calculations:
- SubTotal (TotalPrice) = 150 × 2 = 300 SAR
- FinalPrice = Max(0, 150 - 30) = 120 SAR
- TotalAmount (FinalTotal) = 120 × 2 = 240 SAR
- DiscountAmount (TotalDiscount) = 300 - 240 = 60 SAR
```

---

## ?? How to Use

### For Shopping Cart Display

```csharp
// Get cart with pricing
var cart = await _shoppingCartService.GetUserCartAsync(userId);

// Display
foreach (var item in cart.Items)
{
    Console.WriteLine($"Product: {item.PartName}");
    Console.WriteLine($"Unit Price: {item.UnitPrice} SAR");
    Console.WriteLine($"Discount: {item.DiscountPercent}%");
    Console.WriteLine($"Final Price: {item.FinalPrice} SAR");
    Console.WriteLine($"Quantity: {item.Quantity}");
    Console.WriteLine($"Total: {item.TotalPrice} SAR");
    Console.WriteLine($"Discount Amount: {item.TotalDiscount} SAR");
    Console.WriteLine($"Final Total: {item.FinalTotal} SAR");
}
```

### For Order Creation

```csharp
// Create order from cart
var order = await _orderService.CreateOrderFromCartAsync(userId, request);

// OrderItems automatically calculate prices
// Order totals are sum of OrderItems
Console.WriteLine($"SubTotal: {order.SubTotal} SAR");
Console.WriteLine($"Discount: {order.DiscountAmount} SAR");
Console.WriteLine($"Total: {order.TotalAmount} SAR");
```

### For Price Calculations

```csharp
// Using PricingService directly
var finalPrice = _pricingService.CalculateFinalPrice(
    unitPrice: product.UnitPrice,
    productDiscountPercent: product.DiscountPercent,
    promotion: product.Promotion
);

var totalDiscount = _pricingService.CalculateTotalDiscount(
    unitPrice: product.UnitPrice,
    productDiscountPercent: product.DiscountPercent,
    promotion: product.Promotion,
    quantity: 5
);

var finalTotal = _pricingService.CalculateFinalTotal(
    unitPrice: product.UnitPrice,
    productDiscountPercent: product.DiscountPercent,
    promotion: product.Promotion,
    quantity: 5
);
```

---

## ? Validation Rules

1. **Quantity** must be > 0
2. **UnitPrice** must be ? 0
3. **DiscountPercent** must be 0-100
4. **PromotionDiscountValue**:
   - For Percent: 0-100
   - For Fixed: Any positive amount
5. **FinalPrice** after Fixed discount cannot be negative (use `Math.Max(0, ...)`)

---

## ?? Frontend Display Guidelines

### Cart Item Display

```html
<div class="cart-item">
    <h3>{{ item.partName }}</h3>
    
    <!-- Original Price -->
    <div class="original-price">
        {{ item.unitPrice }} SAR
    </div>
    
    <!-- Show which discount is active -->
    <div class="discount-badge" v-if="item.discountPercent > 0">
        Product Discount: {{ item.discountPercent }}%
    </div>
    <div class="discount-badge promotion" v-else-if="item.hasPromotion">
        Promotion: {{ item.promotionName }}
    </div>
    
    <!-- Final Price -->
    <div class="final-price">
        {{ item.finalPrice }} SAR per unit
    </div>
    
    <!-- Quantity -->
    <div class="quantity">
        Quantity: {{ item.quantity }}
    </div>
    
    <!-- Totals -->
    <div class="totals">
        <div>Subtotal: {{ item.totalPrice }} SAR</div>
        <div class="savings">You Save: {{ item.totalDiscount }} SAR</div>
        <div class="final-total">Total: {{ item.finalTotal }} SAR</div>
    </div>
</div>
```

### Order Summary Display

```html
<div class="order-summary">
    <div class="line-item">
        <span>Subtotal:</span>
        <span>{{ order.subTotal }} SAR</span>
    </div>
    
    <div class="line-item discount">
        <span>Total Discount:</span>
        <span>-{{ order.discountAmount }} SAR</span>
    </div>
    
    <div class="line-item total">
        <span>Total:</span>
        <span>{{ order.totalAmount }} SAR</span>
    </div>
</div>
```

---

## ?? Testing Checklist

- [ ] Product with only product discount
- [ ] Product with only promotion
- [ ] Product with both (product discount should win)
- [ ] Product with percentage promotion
- [ ] Product with fixed amount promotion
- [ ] Multiple quantities
- [ ] Cart with mixed products
- [ ] Order creation from cart
- [ ] Order totals match sum of OrderItems
- [ ] Negative prices handled correctly (Fixed discount)

---

## ?? Key Takeaways

1. ? **One Rule**: Product discount always has priority over promotion
2. ? **Consistent Naming**:
   - `SubTotal` = `TotalPrice` (before discount)
   - `DiscountAmount` = `TotalDiscount`
   - `TotalAmount` = `FinalTotal`
3. ? **Public Properties**: OrderItem pricing properties are public for external calculations
4. ? **Unified Service**: PricingService handles all calculations
5. ? **Automatic Calculation**: OrderItem calculates on construction and quantity updates
6. ? **Order Integrity**: Order totals always match sum of OrderItems

---

## ?? Related Files

- `AutoPartsStore.Core/Entities/OrderItem.cs`
- `AutoPartsStore.Infrastructure/Services/PricingService.cs`
- `AutoPartsStore.Infrastructure/Services/ShoppingCartService.cs`
- `AutoPartsStore.Infrastructure/Repositories/CartItemRepository.cs`
- `AutoPartsStore.Core/Interfaces/IServices/IPricingService.cs`

---

**Last Updated:** January 2025  
**Status:** ? Production Ready
