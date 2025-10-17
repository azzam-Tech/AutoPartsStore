# ?? PRICING SYSTEM - QUICK REFERENCE

## The Golden Rule
```
Product Discount > Promotion > No Discount
```

---

## Price Definitions Cheat Sheet

```csharp
UnitPrice       ? Original price
FinalPrice      ? Price per unit after discount
SubTotal        ? UnitPrice × Quantity (same as TotalPrice)
DiscountAmount  ? Total discount (same as TotalDiscount)
TotalAmount     ? FinalPrice × Quantity (same as FinalTotal)
```

---

## Using PricingService

### New Unified Methods (Recommended)
```csharp
// Calculate final price per unit
var finalPrice = _pricingService.CalculateFinalPrice(
    unitPrice: 500,
    productDiscountPercent: 10,
    promotion: activePromotion
);

// Calculate total discount
var totalDiscount = _pricingService.CalculateTotalDiscount(
    unitPrice: 500,
    productDiscountPercent: 10,
    promotion: activePromotion,
    quantity: 2
);

// Calculate final total
var finalTotal = _pricingService.CalculateFinalTotal(
    unitPrice: 500,
    productDiscountPercent: 10,
    promotion: activePromotion,
    quantity: 2
);
```

### Legacy Methods (Still Supported)
```csharp
var finalPrice = _pricingService.CalculateFinalPrice(
    unitPrice: 500,
    discountType: DiscountType.Percent,
    discountValue: 10
);

var finalTotal = _pricingService.CalculateFinalTotal(
    unitPrice: 500,
    discountType: DiscountType.Percent,
    discountValue: 10,
    quantity: 2
);
```

---

## OrderItem Usage

### Creating OrderItem
```csharp
var orderItem = new OrderItem(
    orderId: 123,
    partId: 456,
    partNumber: "BP-001",
    partName: "Brake Pad",
    unitPrice: 500,
    discountPercent: 10,
    quantity: 2,
    imageUrl: "image.jpg",
    promotionId: 789,
    promotionName: "Summer Sale",
    promotionDiscountType: DiscountType.Percent,
    promotionDiscountValue: 15
);

// Prices are automatically calculated!
// Since discountPercent = 10, it takes priority over promotion
Console.WriteLine($"SubTotal: {orderItem.SubTotal}");        // 1000
Console.WriteLine($"FinalPrice: {orderItem.FinalPrice}");    // 450
Console.WriteLine($"DiscountAmount: {orderItem.DiscountAmount}"); // 100
Console.WriteLine($"TotalAmount: {orderItem.TotalAmount}");  // 900
```

### Updating Quantity
```csharp
orderItem.UpdateQuantity(5);
// All prices recalculate automatically
```

### Manual Recalculation
```csharp
orderItem.DiscountPercent = 15; // Changed externally
orderItem.RecalculateAmounts(); // Recalculate everything
```

---

## Shopping Cart Display

### Backend (ShoppingCartService)
```csharp
var cart = await _shoppingCartService.GetUserCartAsync(userId);

// Cart has:
cart.TotalPrice    // Sum of all UnitPrice × Quantity
cart.TotalDiscount // Sum of all discount amounts
cart.FinalTotal    // Total after all discounts

// Each item has:
foreach (var item in cart.Items)
{
    item.UnitPrice      // Original price
    item.DiscountPercent // Product discount
    item.HasPromotion   // Has active promotion?
    item.PromotionName  // Promotion name if active
    item.FinalPrice     // Price after discount
    item.Quantity       // Quantity
    item.TotalPrice     // UnitPrice × Quantity
    item.TotalDiscount  // Total discount amount
    item.FinalTotal     // Final total for this item
}
```

### Frontend Example (Vue.js)
```vue
<template>
  <div class="cart">
    <div v-for="item in cart.items" :key="item.id" class="cart-item">
      <h3>{{ item.partName }}</h3>
      
      <!-- Show original price -->
      <div class="price">
        <span class="original">{{ item.unitPrice }} SAR</span>
      </div>
      
      <!-- Show active discount -->
      <div v-if="item.discountPercent > 0" class="badge product-discount">
        {{ item.discountPercent }}% OFF
      </div>
      <div v-else-if="item.hasPromotion" class="badge promotion">
        {{ item.promotionName }}
      </div>
      
      <!-- Show final price -->
      <div class="final-price">
        {{ item.finalPrice }} SAR
      </div>
      
      <!-- Quantity -->
      <div class="quantity">
        Qty: {{ item.quantity }}
      </div>
      
      <!-- Totals -->
      <div class="totals">
        <div>Subtotal: {{ item.totalPrice }} SAR</div>
        <div class="savings" v-if="item.totalDiscount > 0">
          You save: {{ item.totalDiscount }} SAR
        </div>
        <div class="total">{{ item.finalTotal }} SAR</div>
      </div>
    </div>
    
    <!-- Cart Summary -->
    <div class="cart-summary">
      <div>Subtotal: {{ cart.totalPrice }} SAR</div>
      <div class="discount">Discount: -{{ cart.totalDiscount }} SAR</div>
      <div class="total">Total: {{ cart.finalTotal }} SAR</div>
    </div>
  </div>
</template>
```

---

## Order Display

### Backend (OrderService)
```csharp
var order = await _orderService.GetOrderByIdAsync(orderId);

// Order has:
order.SubTotal       // Sum of all OrderItems.SubTotal
order.DiscountAmount // Sum of all OrderItems.DiscountAmount
order.TotalAmount    // SubTotal - DiscountAmount

// Each OrderItem has same properties as cart items
foreach (var item in order.Items)
{
    item.UnitPrice      // Original price
    item.FinalPrice     // Price after discount
    item.SubTotal       // UnitPrice × Quantity
    item.DiscountAmount // Total discount
    item.TotalAmount    // Final total
}
```

### Frontend Example
```vue
<template>
  <div class="order-details">
    <h2>Order #{{ order.orderNumber }}</h2>
    
    <!-- Order Items -->
    <div v-for="item in order.items" :key="item.id" class="order-item">
      <div>{{ item.partName }}</div>
      <div>{{ item.unitPrice }} SAR × {{ item.quantity }}</div>
      <div class="discount" v-if="item.discountAmount > 0">
        -{{ item.discountAmount }} SAR
      </div>
      <div class="total">{{ item.totalAmount }} SAR</div>
    </div>
    
    <!-- Order Summary -->
    <div class="order-summary">
      <div>Subtotal: {{ order.subTotal }} SAR</div>
      <div class="discount">Discount: -{{ order.discountAmount }} SAR</div>
      <div class="total">Total: {{ order.totalAmount }} SAR</div>
    </div>
  </div>
</template>
```

---

## Common Patterns

### Pattern 1: Calculate Price for Display
```csharp
// Get product with promotion
var product = await _context.CarParts
    .Include(p => p.Promotion)
    .FirstOrDefaultAsync(p => p.Id == partId);

// Calculate final price
var finalPrice = _pricingService.CalculateFinalPrice(
    product.UnitPrice,
    product.DiscountPercent,
    product.Promotion
);
```

### Pattern 2: Create Order from Cart
```csharp
// Cart items are converted to OrderItems
// OrderItems automatically calculate prices
// Order totals are recalculated from OrderItems

var order = await _orderService.CreateOrderFromCartAsync(userId, request);
// All prices are correct and consistent!
```

### Pattern 3: Update OrderItem Quantity
```csharp
orderItem.UpdateQuantity(newQuantity);
// SubTotal, DiscountAmount, and TotalAmount recalculate automatically
```

---

## Testing Checklist

```csharp
// Test Case 1: Product discount only
UnitPrice: 500, DiscountPercent: 10%, Promotion: null, Qty: 2
Expected: FinalTotal = 900

// Test Case 2: Promotion only
UnitPrice: 300, DiscountPercent: 0, Promotion: 15%, Qty: 3
Expected: FinalTotal = 765

// Test Case 3: Both (product wins)
UnitPrice: 200, DiscountPercent: 20%, Promotion: 25%, Qty: 4
Expected: FinalTotal = 640 (uses 20%, not 25%)

// Test Case 4: Fixed promotion
UnitPrice: 150, DiscountPercent: 0, Promotion: 30 SAR, Qty: 2
Expected: FinalTotal = 240
```

---

## Troubleshooting

### Issue: Prices don't match
**Solution:** Ensure you're using the PRIORITY rule:
```csharp
if (DiscountPercent > 0)
    // Use product discount
else if (Promotion != null && Promotion.IsActiveNow())
    // Use promotion
else
    // No discount
```

### Issue: Order totals don't match OrderItems
**Solution:** Recalculate order totals:
```csharp
var subTotal = orderItems.Sum(oi => oi.SubTotal);
var discount = orderItems.Sum(oi => oi.DiscountAmount);
order.UpdateTotals(subTotal, discount);
```

### Issue: Cart shows wrong discount
**Solution:** Check promotion is active:
```csharp
if (promotion != null && promotion.IsActiveNow())
```

---

## Key Files

```
Core/
??? Entities/
?   ??? OrderItem.cs                    ? Pricing properties
??? Interfaces/IServices/
?   ??? IPricingService.cs             ? Interface

Infrastructure/
??? Services/
?   ??? PricingService.cs              ? Main logic
?   ??? ShoppingCartService.cs         ? Cart pricing
?   ??? OrderService.cs                ? Order creation
??? Repositories/
    ??? CartItemRepository.cs          ? Cart item pricing
```

---

## Quick Commands

```bash
# Build and verify
dotnet build

# Run tests
dotnet test

# Check specific file
dotnet build AutoPartsStore.Infrastructure/Services/PricingService.cs
```

---

**Remember:** Product Discount > Promotion > No Discount ??
