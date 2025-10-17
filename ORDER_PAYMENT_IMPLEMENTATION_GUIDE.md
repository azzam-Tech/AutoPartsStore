# Order Management & Payment Integration Implementation Guide

## Overview
This document outlines the complete implementation of Order Management System with Moyasar Payment Gateway integration for the AutoPartsStore project.

## ? Completed Components

### 1. Core Entities
- ? `Order.cs` - Main order entity with status tracking
- ? `OrderItem.cs` - Order line items with promotion/discount support
- ? `PaymentTransaction.cs` - Payment tracking entity

### 2. Enums
- ? `OrderStatus` - Pending, PaymentPending, Paid, Processing, Shipped, Delivered, Cancelled, Refunded, Failed
- ? `PaymentStatus` - Initiated, Pending, Paid, Failed, Authorized, Captured, Refunded, PartiallyRefunded, Voided
- ? `PaymentMethod` - CreditCard, Mada, ApplePay, STCPay, Tabby, Tamara

### 3. DTOs & Request Models
- ? `OrderDto.cs`, `OrderItemDto.cs`, `OrderSummaryDto.cs`
- ? `OrderRequests.cs` - Create, Update, Cancel, Filter
- ? `PaymentDto.cs` - Transaction and summary DTOs
- ? `PaymentRequests.cs` - Initiate, Callback, Refund
- ? `MoyasarModels.cs` - Full Moyasar API models

### 4. EF Core Configurations
- ? `OrderConfiguration.cs`
- ? `OrderItemConfiguration.cs`
- ? `PaymentTransactionConfiguration.cs`

### 5. Database Context
- ? Updated `AppDbContext.cs` with new DbSets

### 6. Configuration
- ? `MoyasarSettings.cs`
- ? Updated `appsettings.json` with Moyasar settings

### 7. Interfaces
- ? `IOrderRepository.cs`
- ? `IPaymentTransactionRepository.cs`
- ? `IOrderService.cs`
- ? `IPaymentService.cs`
- ? `IMoyasarService.cs`

## ?? Next Steps - Implementation Required

### Step 1: Install Required NuGet Packages
```bash
# In AutoPartsStore.Infrastructure project
dotnet add package System.Net.Http.Json
```

### Step 2: Create Repositories

#### File: `AutoPartsStore.Infrastructure/Repositories/OrderRepository.cs`
```csharp
public class OrderRepository : BaseRepository<Order>, IOrderRepository
{
    // Implement all interface methods
    // Include projection to OrderDto
    // Add filtering, sorting, pagination
}
```

#### File: `AutoPartsStore.Infrastructure/Repositories/PaymentTransactionRepository.cs`
```csharp
public class PaymentTransactionRepository : BaseRepository<PaymentTransaction>, IPaymentTransactionRepository
{
    // Implement all interface methods
    // Include projection to PaymentTransactionDto
}
```

### Step 3: Create Moyasar Service

#### File: `AutoPartsStore.Infrastructure/Services/MoyasarService.cs`
```csharp
public class MoyasarService : IMoyasarService
{
    private readonly HttpClient _httpClient;
    private readonly MoyasarSettings _settings;
    private readonly ILogger<MoyasarService> _logger;

    // Key Methods:
    // - CreatePaymentAsync() - POST /v1/payments
    // - GetPaymentAsync() - GET /v1/payments/{id}
    // - RefundPaymentAsync() - POST /v1/payments/{id}/refund
    // - Handle authentication (Basic Auth with ApiKey)
    // - Convert amounts (SAR to halalas: amount * 100)
    // - Handle errors and exceptions
}
```

### Step 4: Create Order Service

#### File: `AutoPartsStore.Infrastructure/Services/OrderService.cs`
```csharp
public class OrderService : IOrderService
{
    // Key Methods:
    // - CreateOrderFromCartAsync() - Convert cart to order
    // - CreateOrderAsync() - Create order from items
    // - Calculate totals (SubTotal, Discount, Tax, Shipping)
    // - Validate stock availability
    // - Reduce stock on order creation
    // - Handle order status transitions
    // - Generate order number
}
```

### Step 5: Create Payment Service

#### File: `AutoPartsStore.Infrastructure/Services/PaymentService.cs`
```csharp
public class PaymentService : IPaymentService
{
    private readonly IMoyasarService _moyasarService;
    private readonly IOrderService _orderService;
    // Implement payment flow:
    // 1. InitiatePaymentAsync - Create payment transaction
    // 2. Call Moyasar API
    // 3. ProcessPaymentCallbackAsync - Handle webhook
    // 4. Update order status based on payment
    // 5. Handle refunds
}
```

### Step 6: Create Controllers

#### File: `AutoPartsStore.Web/Controllers/OrdersController.cs`
```csharp
[ApiController]
[Route("api/orders")]
public class OrdersController : BaseController
{
    // Endpoints:
    // POST /api/orders - Create from cart
    // GET /api/orders - List orders (admin)
    // GET /api/orders/{id} - Get order details
    // GET /api/orders/user/{userId} - User orders
    // PATCH /api/orders/{id}/status - Update status (admin)
    // PATCH /api/orders/{id}/cancel - Cancel order
    // PATCH /api/orders/{id}/tracking - Update tracking
}
```

#### File: `AutoPartsStore.Web/Controllers/PaymentsController.cs`
```csharp
[ApiController]
[Route("api/payments")]
public class PaymentsController : BaseController
{
    // Endpoints:
    // POST /api/payments/initiate - Start payment
    // POST /api/payments/callback - Moyasar webhook
    // GET /api/payments/{id} - Get payment details
    // GET /api/payments/order/{orderId} - Get by order
    // POST /api/payments/{id}/refund - Process refund (admin)
    // GET /api/payments/summary - Payment statistics (admin)
}
```

### Step 7: Register Services in Program.cs
```csharp
// Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();

// Services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IMoyasarService, MoyasarService>();

// Moyasar Settings
builder.Services.Configure<MoyasarSettings>(
    builder.Configuration.GetSection("Moyasar"));

// HttpClient for Moyasar
builder.Services.AddHttpClient<IMoyasarService, MoyasarService>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<MoyasarSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    
    // Basic Authentication
    var authToken = Convert.ToBase64String(
        Encoding.ASCII.GetBytes($"{settings.ApiKey}:"));
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Basic", authToken);
});
```

### Step 8: Create Database Migration
```bash
dotnet ef migrations add AddOrderAndPaymentTables --project AutoPartsStore.Infrastructure --startup-project AutoPartsStore.Web

dotnet ef database update --project AutoPartsStore.Infrastructure --startup-project AutoPartsStore.Web
```

## ?? Key Features

### Order Features
1. **Create Order from Shopping Cart**
   - Validates cart items
   - Checks stock availability
   - Calculates totals with discounts and promotions
   - Applies 15% VAT
   - Includes shipping cost

2. **Order Item Details**
   - Captures product snapshot
   - Stores both product discount and promotion discount
   - Calculates best discount automatically
   - Tracks promotion information

3. **Order Status Workflow**
   ```
   Pending ? PaymentPending ? Paid ? Processing ? Shipped ? Delivered
                ?              ?         ?          ?
            Cancelled      Failed   Refunded    Cancelled
   ```

4. **Order Management**
   - Cancel orders (before shipped)
   - Add tracking numbers
   - Admin notes
   - Order history

### Payment Features
1. **Moyasar Integration**
   - Credit/Debit Cards (Visa, MasterCard)
   - Mada (Saudi local cards)
   - Apple Pay
   - STC Pay
   - Tabby (BNPL)
   - Tamara (BNPL)

2. **Payment Flow**
   ```
   1. User completes order
   2. System creates PaymentTransaction (Initiated)
   3. System calls Moyasar API
   4. Moyasar processes payment
   5. Moyasar sends webhook callback
   6. System updates payment status
   7. System updates order status
   8. Stock is reduced (on successful payment)
   ```

3. **Security**
   - Never store full card numbers
   - Only last 4 digits stored
   - All sensitive data in Moyasar
   - Webhook signature verification (implement)

4. **Refund Support**
   - Full refunds
   - Partial refunds
   - Automatic order status update
   - Stock restoration (optional)

## ?? Amount Calculations

### Order Total Calculation
```csharp
SubTotal = Sum of (UnitPrice * Quantity) for all items
DiscountAmount = Sum of discounts from items
TaxAmount = (SubTotal - DiscountAmount) * 0.15  // 15% VAT
TotalAmount = (SubTotal - DiscountAmount) + TaxAmount + ShippingCost
```

### Order Item Calculation
```csharp
SubTotal = UnitPrice * Quantity

// Apply product discount
PriceAfterProductDiscount = UnitPrice * (1 - DiscountPercent / 100)

// Apply promotion discount (if better)
If Promotion exists:
    If PromotionType == Percent:
        PriceAfterPromotion = UnitPrice * (1 - PromotionValue / 100)
    Else If PromotionType == Fixed:
        PriceAfterPromotion = UnitPrice - PromotionValue

// Use best price
FinalPrice = Min(PriceAfterProductDiscount, PriceAfterPromotion)
TotalAmount = FinalPrice * Quantity
DiscountAmount = SubTotal - TotalAmount
```

### Moyasar Amount Conversion
```csharp
// Moyasar uses halalas (smallest currency unit)
// 1 SAR = 100 halalas
MoyasarAmount = TotalAmount * 100
```

## ?? Security Considerations

1. **API Keys**
   - Store in environment variables
   - Never commit to source control
   - Use different keys for test/production

2. **Webhook Verification**
   - Implement signature verification
   - Validate payment status from Moyasar
   - Prevent replay attacks

3. **Order Validation**
   - Verify user owns the order
   - Check order status before operations
   - Validate payment amounts match

4. **Stock Management**
   - Reserve stock on order creation
   - Reduce stock only on successful payment
   - Restore stock on cancellation/refund

## ?? Database Indexes

Already configured in entity configurations:
- Orders: OrderNumber (unique), UserId, Status, OrderDate
- OrderItems: OrderId, PartId
- PaymentTransactions: MoyasarPaymentId, TransactionReference (unique), OrderId, UserId, Status

## ?? Testing Checklist

### Order Testing
- [ ] Create order from empty cart (should fail)
- [ ] Create order from cart with out-of-stock items (should fail)
- [ ] Create order successfully
- [ ] Cancel order (various statuses)
- [ ] Update tracking number
- [ ] Get user orders
- [ ] Get order by ID
- [ ] Filter orders

### Payment Testing
- [ ] Initiate payment with credit card
- [ ] Initiate payment with Mada
- [ ] Process successful payment callback
- [ ] Process failed payment callback
- [ ] Verify payment status
- [ ] Full refund
- [ ] Partial refund
- [ ] Get payment history

### Integration Testing
- [ ] Complete order flow (cart ? order ? payment ? delivered)
- [ ] Failed payment flow
- [ ] Cancelled order flow
- [ ] Refund flow

## ?? Moyasar API Reference

- Base URL: `https://api.moyasar.com/v1`
- Authentication: Basic Auth (API Key as username, empty password)
- Documentation: https://docs.moyasar.com/api/payments/01-create-payment

### Key Endpoints
- `POST /payments` - Create payment
- `GET /payments/{id}` - Fetch payment
- `PUT /payments/{id}` - Update payment
- `POST /payments/{id}/refund` - Refund payment
- `POST /payments/{id}/capture` - Capture authorized payment
- `POST /payments/{id}/void` - Void payment

### Payment Statuses
- `initiated` - Payment created
- `paid` - Payment successful
- `failed` - Payment failed
- `authorized` - Authorized but not captured
- `captured` - Payment captured
- `refunded` - Payment refunded
- `voided` - Payment voided

## ?? Next Actions

1. Implement repositories (OrderRepository, PaymentTransactionRepository)
2. Implement MoyasarService with HTTP client
3. Implement OrderService with business logic
4. Implement PaymentService integrating Moyasar
5. Create controllers with proper authorization
6. Register all services in Program.cs
7. Run migration to create database tables
8. Test with Moyasar test credentials
9. Update credentials for production

Would you like me to proceed with implementing any specific component?
