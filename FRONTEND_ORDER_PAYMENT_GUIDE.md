# AutoPartsStore - Order & Payment API Documentation for Frontend

## ?? Table of Contents
1. [Overview](#overview)
2. [Order Management System](#order-management-system)
3. [Payment Integration](#payment-integration)
4. [Complete User Flow](#complete-user-flow)
5. [API Endpoints Reference](#api-endpoints-reference)
6. [Request/Response Examples](#requestresponse-examples)
7. [Error Handling](#error-handling)
8. [Testing Guide](#testing-guide)

---

## Overview

The AutoPartsStore backend now includes a complete Order Management and Payment Integration system with:

- ? Full order lifecycle management
- ? Moyasar payment gateway integration
- ? Support for multiple payment methods (Mada, Visa, MasterCard, ApplePay, STC Pay, Tabby, Tamara)
- ? Automatic stock management
- ? Order tracking and status updates
- ? Refund processing
- ? Comprehensive order and payment history

**Base API URL:** `https://yourdomain.com/api`

**Authentication:** All endpoints require JWT Bearer token except where noted as `[Public]`

---

## Order Management System

### Order Status Workflow

Orders follow this lifecycle:

```
Pending ? PaymentPending ? Paid ? Processing ? Shipped ? Delivered
   ?            ?            ?         ?          ?
Cancelled    Failed      Refunded  Cancelled  Cancelled
```

**Status Codes:**
- `0` = Pending (Order created, awaiting payment)
- `1` = PaymentPending (Payment process started)
- `2` = Paid (Payment successful)
- `3` = Processing (Order being prepared)
- `4` = Shipped (Order dispatched)
- `5` = Delivered (Order completed)
- `6` = Cancelled (Order cancelled)
- `7` = Refunded (Payment refunded)
- `8` = Failed (Payment failed)

### Order Calculations

**Order Total Formula:**
```javascript
SubTotal = Sum of (UnitPrice ◊ Quantity) for all items
DiscountAmount = Sum of all item discounts
TaxAmount = (SubTotal - DiscountAmount) ◊ 0.15  // 15% VAT
ShippingCost = 25.00 SAR  // Fixed for now
TotalAmount = (SubTotal - DiscountAmount) + TaxAmount + ShippingCost
```

**Order Item Calculation:**
```javascript
// Each item can have:
// 1. Product direct discount (DiscountPercent)
// 2. Promotion discount (if product has active promotion)

SubTotal = UnitPrice ◊ Quantity

// Apply product discount
PriceAfterDiscount = UnitPrice ◊ (1 - DiscountPercent / 100)

// Apply promotion discount (if exists)
if (Promotion) {
  if (PromotionType === "Percent") {
    PriceAfterPromotion = UnitPrice ◊ (1 - PromotionValue / 100)
  } else if (PromotionType === "Fixed") {
    PriceAfterPromotion = UnitPrice - PromotionValue
  }
  
  // Use best discount
  FinalPrice = Math.min(PriceAfterDiscount, PriceAfterPromotion)
} else {
  FinalPrice = PriceAfterDiscount
}

TotalAmount = FinalPrice ◊ Quantity
DiscountAmount = SubTotal - TotalAmount
```

---

## Payment Integration

### Supported Payment Methods

**Payment Method Codes:**
- `0` = CreditCard (Visa/MasterCard)
- `1` = Mada (Saudi local cards)
- `2` = ApplePay
- `3` = STCPay
- `4` = Tabby (Buy Now, Pay Later)
- `5` = Tamara (Buy Now, Pay Later)

### Payment Status Codes

- `0` = Initiated (Payment created but not processed)
- `1` = Pending (Payment processing)
- `2` = Paid (Payment successful)
- `3` = Failed (Payment failed)
- `4` = Authorized (Payment authorized but not captured)
- `5` = Captured (Payment captured)
- `6` = Refunded (Full refund)
- `7` = PartiallyRefunded (Partial refund)
- `8` = Voided (Payment voided)

### Payment Flow

```
1. User adds items to cart
2. User proceeds to checkout
3. Frontend calls POST /api/orders/from-cart
4. Backend creates order with status: Pending
5. Frontend calls POST /api/payments/initiate
6. Backend creates payment transaction and calls Moyasar API
7. Order status changes to: PaymentPending
8. Moyasar processes payment
9. Moyasar sends webhook to POST /api/payments/callback
10. If successful:
    - Payment status: Paid
    - Order status: Paid
    - Stock reduced automatically
11. If failed:
    - Payment status: Failed
    - Order status: Failed
```

---

## Complete User Flow

### Step 1: Create Order from Cart

**Endpoint:** `POST /api/orders/from-cart`

**Headers:**
```json
{
  "Authorization": "Bearer {JWT_TOKEN}",
  "Content-Type": "application/json"
}
```

**Request Body:**
```json
{
  "shippingAddressId": 1,
  "customerNotes": "Please deliver before 5 PM"
}
```

**Note:** ?? **You do NOT need to send cart items!** The backend automatically fetches items from your shopping cart.

**Response (Success - 200):**
```json
{
  "success": true,
  "message": " „ ≈‰‘«¡ «·ÿ·» »‰Ã«Õ „‰ ”·… «· ”Êﬁ.",
  "data": {
    "id": 123,
    "orderNumber": "ORD-20250106-45678",
    "userId": 5,
    "userName": "Ahmed Mohammed",
    "userEmail": "ahmed@example.com",
    "shippingAddressId": 1,
    "shippingAddress": "King Fahd Road 123, Al-Malaz, Riyadh",
    "subTotal": 1500.00,
    "discountAmount": 150.00,
    "taxAmount": 202.50,
    "shippingCost": 25.00,
    "totalAmount": 1577.50,
    "status": 0,
    "statusText": "Pending",
    "orderDate": "2025-01-06T10:30:00Z",
    "paidDate": null,
    "shippedDate": null,
    "deliveredDate": null,
    "cancelledDate": null,
    "cancellationReason": null,
    "paymentTransactionId": null,
    "paymentStatus": null,
    "paymentMethod": null,
    "customerNotes": "Please deliver before 5 PM",
    "adminNotes": null,
    "trackingNumber": null,
    "items": [
      {
        "id": 1,
        "orderId": 123,
        "partId": 45,
        "partNumber": "BRK-001",
        "partName": "Brake Pads - Front",
        "imageUrl": "/images/brake-pads.jpg",
        "unitPrice": 500.00,
        "discountPercent": 10.0,
        "quantity": 2,
        "promotionId": 5,
        "promotionName": "Summer Sale",
        "promotionDiscountType": "Percent",
        "promotionDiscountValue": 15.0,
        "subTotal": 1000.00,
        "discountAmount": 150.00,
        "finalPrice": 425.00,
        "totalAmount": 850.00
      },
      {
        "id": 2,
        "orderId": 123,
        "partId": 67,
        "partNumber": "OIL-002",
        "partName": "Engine Oil Filter",
        "imageUrl": "/images/oil-filter.jpg",
        "unitPrice": 250.00,
        "discountPercent": 0,
        "quantity": 2,
        "promotionId": null,
        "promotionName": null,
        "promotionDiscountType": null,
        "promotionDiscountValue": null,
        "subTotal": 500.00,
        "discountAmount": 0,
        "finalPrice": 250.00,
        "totalAmount": 500.00
      }
    ],
    "totalItems": 2,
    "totalQuantity": 4
  },
  "errors": []
}
```

**Important Notes:**
- Cart is automatically cleared after successful order creation
- Stock is NOT reduced at this point (only after successful payment)
- Order status starts as `Pending`

### Step 2: Initiate Payment

**Endpoint:** `POST /api/payments/initiate`

**Headers:**
```json
{
  "Authorization": "Bearer {JWT_TOKEN}",
  "Content-Type": "application/json"
}
```

**Request Body (Credit Card):**
```json
{
  "orderId": 123,
  "paymentMethod": 0,
  "cardNumber": "4111111111111111",
  "cardHolderName": "Ahmed Mohammed",
  "expiryMonth": "12",
  "expiryYear": "2025",
  "cvv": "123",
  "callbackUrl": "https://yourdomain.com/payment-result"
}
```

**Request Body (Mada):**
```json
{
  "orderId": 123,
  "paymentMethod": 1,
  "cardNumber": "5123450000000008",
  "cardHolderName": "Ahmed Mohammed",
  "expiryMonth": "12",
  "expiryYear": "2025",
  "cvv": "123",
  "callbackUrl": "https://yourdomain.com/payment-result"
}
```

**Request Body (Apple Pay):**
```json
{
  "orderId": 123,
  "paymentMethod": 2,
  "callbackUrl": "https://yourdomain.com/payment-result"
}
```

**Request Body (Tabby/Tamara):**
```json
{
  "orderId": 123,
  "paymentMethod": 4,
  "callbackUrl": "https://yourdomain.com/payment-result"
}
```

**Response (Success - 200):**
```json
{
  "success": true,
  "message": " „ »œ¡ ⁄„·Ì… «·œ›⁄ »‰Ã«Õ.",
  "data": {
    "paymentId": "pay_abc123xyz789",
    "status": "initiated",
    "amount": 1577.50,
    "currency": "SAR",
    "transactionUrl": {
      "type": "redirect",
      "value": "https://api.moyasar.com/v1/payments/pay_abc123xyz789/authenticate"
    }
  },
  "errors": []
}
```

**What to do with the response:**
1. If `transactionUrl` exists and type is `redirect`:
   - Redirect user to `transactionUrl.value`
   - User completes payment on Moyasar's page
   - Moyasar redirects back to your `callbackUrl`
2. If payment is immediate (some methods):
   - Payment is processed automatically
   - Wait for webhook callback

### Step 3: Handle Payment Result

After payment processing, Moyasar will:
1. Send a webhook to your backend (`POST /api/payments/callback`)
2. Redirect user to your `callbackUrl` with payment status

**Your callback URL will receive:**
```
GET https://yourdomain.com/payment-result?id=pay_abc123xyz789&status=paid
```

**Frontend should:**
1. Extract `id` (Moyasar payment ID) from URL
2. Call verify endpoint to get final status

### Step 4: Verify Payment Status

**Endpoint:** `POST /api/payments/verify/{moyasarPaymentId}`

**Headers:**
```json
{
  "Authorization": "Bearer {JWT_TOKEN}"
}
```

**Example:** `POST /api/payments/verify/pay_abc123xyz789`

**Response (Success - 200):**
```json
{
  "success": true,
  "message": " „ «· Õﬁﬁ „‰ Õ«·… «·œ›⁄.",
  "data": {
    "id": 45,
    "orderId": 123,
    "orderNumber": "ORD-20250106-45678",
    "userId": 5,
    "userName": "Ahmed Mohammed",
    "moyasarPaymentId": "pay_abc123xyz789",
    "transactionReference": "TXN-20250106-12345",
    "paymentMethod": 0,
    "paymentMethodText": "CreditCard",
    "status": 2,
    "statusText": "Paid",
    "amount": 1577.50,
    "currency": "SAR",
    "authorizationCode": "AUTH-123456",
    "errorMessage": null,
    "errorCode": null,
    "cardLast4": "1111",
    "cardBrand": "Visa",
    "refundedAmount": null,
    "refundedDate": null,
    "refundReason": null,
    "initiatedDate": "2025-01-06T10:35:00Z",
    "completedDate": "2025-01-06T10:36:30Z",
    "failedDate": null
  },
  "errors": []
}
```

### Step 5: Show Order Confirmation

After successful payment, show order details:

**Endpoint:** `GET /api/orders/{orderId}`

**Headers:**
```json
{
  "Authorization": "Bearer {JWT_TOKEN}"
}
```

**Response:** (Same structure as Step 1, but with updated status)

---

## API Endpoints Reference

### Order Endpoints

#### 1. Create Order from Cart
```
POST /api/orders/from-cart
Authorization: Bearer {token}
```
**Body:**
```json
{
  "shippingAddressId": number,
  "customerNotes": string (optional)
}
```

#### 2. Create Order Directly
```
POST /api/orders
Authorization: Bearer {token}
```
**Body:**
```json
{
  "shippingAddressId": number,
  "customerNotes": string (optional),
  "items": [
    {
      "partId": number,
      "quantity": number
    }
  ]
}
```

#### 3. Get Order by ID
```
GET /api/orders/{id}
Authorization: Bearer {token}
```

#### 4. Get Order by Number
```
GET /api/orders/number/{orderNumber}
Authorization: Bearer {token}
```

#### 5. Get My Orders
```
GET /api/orders/my-orders
Authorization: Bearer {token}
```
**Response:** Array of OrderSummaryDto
```json
[
  {
    "id": 123,
    "orderNumber": "ORD-20250106-45678",
    "status": 2,
    "statusText": "Paid",
    "orderDate": "2025-01-06T10:30:00Z",
    "totalAmount": 1577.50,
    "totalItems": 2
  }
]
```

#### 6. Cancel Order
```
PATCH /api/orders/{id}/cancel
Authorization: Bearer {token}
```
**Body:**
```json
{
  "reason": "Changed my mind" (minimum 5 characters)
}
```

#### 7. Calculate Order Total
```
POST /api/orders/calculate-total
Authorization: Bearer {token}
```
**Body:**
```json
[
  {
    "partId": 45,
    "quantity": 2
  },
  {
    "partId": 67,
    "quantity": 1
  }
}
```
**Response:**
```json
{
  "success": true,
  "data": {
    "total": 1577.50
  }
}
```

### Payment Endpoints

#### 1. Initiate Payment
```
POST /api/payments/initiate
Authorization: Bearer {token}
```
**Body:** See Step 2 examples above

#### 2. Verify Payment
```
POST /api/payments/verify/{moyasarPaymentId}
Authorization: Bearer {token}
```

#### 3. Get Payment by Order ID
```
GET /api/payments/order/{orderId}
Authorization: Bearer {token}
```

#### 4. Get My Payments
```
GET /api/payments/my-payments
Authorization: Bearer {token}
```
**Response:** Array of PaymentTransactionDto

#### 5. Payment Callback (Webhook)
```
POST /api/payments/callback
[Public - No Authorization Required]
```
**Note:** This endpoint is called by Moyasar automatically. Don't call it from frontend.

### Admin Endpoints

#### 1. Get All Orders
```
GET /api/orders?page=1&pageSize=20&status=2&userId=5&fromDate=2025-01-01
Authorization: Bearer {token}
Role: Admin
```

#### 2. Update Order Status
```
PATCH /api/orders/{id}/status
Authorization: Bearer {token}
Role: Admin
```
**Body:**
```json
{
  "orderStatus": 3,
  "notes": "Order is being prepared"
}
```

#### 3. Update Tracking Number
```
PATCH /api/orders/{id}/tracking
Authorization: Bearer {token}
Role: Admin
```
**Body:**
```json
{
  "trackingNumber": "TRACK-123456789"
}
```

#### 4. Process Refund
```
POST /api/payments/{paymentId}/refund
Authorization: Bearer {token}
Role: Admin
```
**Body:**
```json
{
  "amount": 500.00,
  "reason": "Product defect"
}
```

#### 5. Get Order Statistics
```
GET /api/orders/statistics?fromDate=2025-01-01&toDate=2025-01-31
Authorization: Bearer {token}
Role: Admin
```

#### 6. Get Payment Summary
```
GET /api/payments/summary?fromDate=2025-01-01&toDate=2025-01-31
Authorization: Bearer {token}
Role: Admin
```

---

## Request/Response Examples

### Error Responses

All error responses follow this format:

```json
{
  "success": false,
  "message": "Error message in Arabic",
  "data": null,
  "errors": ["Detailed error 1", "Detailed error 2"]
}
```

**Common HTTP Status Codes:**
- `200` - Success
- `400` - Bad Request (validation error)
- `401` - Unauthorized (missing or invalid token)
- `403` - Forbidden (insufficient permissions)
- `404` - Not Found
- `409` - Conflict (e.g., duplicate order)
- `500` - Internal Server Error

### Example: Empty Cart Error

```json
{
  "success": false,
  "message": "”·… «· ”Êﬁ ›«—€…. ·« Ì„ﬂ‰ ≈‰‘«¡ ÿ·».",
  "data": null,
  "errors": []
}
```

### Example: Out of Stock Error

```json
{
  "success": false,
  "message": "»⁄÷ «·„‰ Ã«  €Ì— „ Ê›—… »«·ﬂ„Ì… «·„ÿ·Ê»….",
  "data": null,
  "errors": [
    "Brake Pads - Front - «·ﬂ„Ì… «·„ «Õ…: 1, «·„ÿ·Ê»…: 2"
  ]
}
```

### Example: Payment Failed

```json
{
  "success": false,
  "message": "›‘·  ⁄„·Ì… «·œ›⁄.",
  "data": {
    "orderId": 123,
    "paymentStatus": "Failed",
    "errorMessage": "Insufficient funds",
    "errorCode": "card_declined"
  },
  "errors": []
}
```

---

## Error Handling

### Frontend Error Handling Guide

#### 1. Network Errors
```javascript
try {
  const response = await fetch('/api/orders/from-cart', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(orderData)
  });
  
  if (!response.ok) {
    const error = await response.json();
    // Handle error based on status code
    if (response.status === 401) {
      // Redirect to login
    } else if (response.status === 400) {
      // Show validation errors
      showErrors(error.errors);
    } else {
      // Show general error message
      showError(error.message);
    }
  }
  
  const result = await response.json();
  if (result.success) {
    // Handle success
  }
} catch (error) {
  // Handle network error
  showError('Network error. Please check your connection.');
}
```

#### 2. Validation Errors

When creating an order, validate on frontend before sending:

```javascript
function validateOrderCreation(shippingAddressId, cart) {
  const errors = [];
  
  if (!shippingAddressId) {
    errors.push('Ì—ÃÏ «Œ Ì«— ⁄‰Ê«‰ «· Ê’Ì·');
  }
  
  if (!cart || cart.items.length === 0) {
    errors.push('”·… «· ”Êﬁ ›«—€…');
  }
  
  cart.items.forEach(item => {
    if (item.quantity > item.availableStock) {
      errors.push(`${item.partName} - «·ﬂ„Ì… «·„ÿ·Ê»…   Ã«Ê“ «·„ «Õ`);
    }
  });
  
  return errors;
}
```

#### 3. Payment Errors

```javascript
async function handlePaymentResult(moyasarPaymentId) {
  const payment = await verifyPayment(moyasarPaymentId);
  
  switch(payment.status) {
    case 2: // Paid
      showSuccess(' „  ⁄„·Ì… «·œ›⁄ »‰Ã«Õ');
      redirectToOrderConfirmation(payment.orderId);
      break;
      
    case 3: // Failed
      showError(payment.errorMessage || '›‘·  ⁄„·Ì… «·œ›⁄');
      redirectToOrderDetails(payment.orderId);
      break;
      
    case 1: // Pending
      showInfo('⁄„·Ì… «·œ›⁄ ﬁÌœ «·„⁄«·Ã…');
      pollPaymentStatus(moyasarPaymentId);
      break;
      
    default:
      showError('Õ«·… «·œ›⁄ €Ì— „⁄—Ê›…');
  }
}
```

---

## Testing Guide

### Test Data

**Test Credit Cards (Moyasar Test Mode):**

| Card Type | Number | CVV | Expiry | Result |
|-----------|--------|-----|--------|--------|
| Visa | 4111111111111111 | Any | Future | Success |
| MasterCard | 5123450000000008 | Any | Future | Success |
| Visa | 4000000000000002 | Any | Future | Declined |
| Mada | 5297410000000000 | Any | Future | Success |

**Test Amount:** Any amount works in test mode

### Testing Workflow

#### Test 1: Successful Order Flow
1. Add products to cart
2. Create order from cart
3. Verify order created with status `Pending`
4. Initiate payment with test card `4111111111111111`
5. Complete payment on Moyasar page
6. Verify payment status is `Paid`
7. Verify order status changed to `Paid`
8. Check stock reduced

#### Test 2: Failed Payment
1. Create order
2. Initiate payment with declined card `4000000000000002`
3. Verify payment fails
4. Verify order status is `Failed`
5. Verify stock NOT reduced

#### Test 3: Order Cancellation
1. Create order
2. Don't pay
3. Cancel order with reason
4. Verify order status is `Cancelled`

#### Test 4: Refund Process
1. Complete successful order
2. Admin processes refund
3. Verify payment status updated
4. Verify order status is `Refunded`
5. Verify stock restored

### Postman Collection

Import this collection for easy testing:

```json
{
  "info": {
    "name": "AutoPartsStore - Orders & Payments",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "1. Create Order from Cart",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"shippingAddressId\": 1,\n  \"customerNotes\": \"Test order\"\n}",
          "options": {
            "raw": {
              "language": "json"
            }
          }
        },
        "url": {
          "raw": "{{baseUrl}}/api/orders/from-cart",
          "host": ["{{baseUrl}}"],
          "path": ["api", "orders", "from-cart"]
        }
      }
    },
    {
      "name": "2. Initiate Payment",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"orderId\": {{orderId}},\n  \"paymentMethod\": 0,\n  \"cardNumber\": \"4111111111111111\",\n  \"cardHolderName\": \"Test User\",\n  \"expiryMonth\": \"12\",\n  \"expiryYear\": \"2025\",\n  \"cvv\": \"123\",\n  \"callbackUrl\": \"http://localhost:3000/payment-result\"\n}",
          "options": {
            "raw": {
              "language": "json"
            }
          }
        },
        "url": {
          "raw": "{{baseUrl}}/api/payments/initiate",
          "host": ["{{baseUrl}}"],
          "path": ["api", "payments", "initiate"]
        }
      }
    },
    {
      "name": "3. Verify Payment",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          }
        ],
        "url": {
          "raw": "{{baseUrl}}/api/payments/verify/{{moyasarPaymentId}}",
          "host": ["{{baseUrl}}"],
          "path": ["api", "payments", "verify", "{{moyasarPaymentId}}"]
        }
      }
    },
    {
      "name": "4. Get My Orders",
      "request": {
        "method": "GET",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          }
        ],
        "url": {
          "raw": "{{baseUrl}}/api/orders/my-orders",
          "host": ["{{baseUrl}}"],
          "path": ["api", "orders", "my-orders"]
        }
      }
    },
    {
      "name": "5. Get Order Details",
      "request": {
        "method": "GET",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          }
        ],
        "url": {
          "raw": "{{baseUrl}}/api/orders/{{orderId}}",
          "host": ["{{baseUrl}}"],
          "path": ["api", "orders", "{{orderId}}"]
        }
      }
    }
  ]
}
```

---

## Frontend Implementation Example

### React/Vue/Angular Example

```javascript
// Order Service
class OrderService {
  constructor(apiClient) {
    this.api = apiClient;
  }

  async createOrderFromCart(shippingAddressId, customerNotes = '') {
    return await this.api.post('/api/orders/from-cart', {
      shippingAddressId,
      customerNotes
    });
  }

  async getOrderById(orderId) {
    return await this.api.get(`/api/orders/${orderId}`);
  }

  async getMyOrders() {
    return await this.api.get('/api/orders/my-orders');
  }

  async cancelOrder(orderId, reason) {
    return await this.api.patch(`/api/orders/${orderId}/cancel`, {
      reason
    });
  }

  async calculateTotal(items) {
    return await this.api.post('/api/orders/calculate-total', items);
  }
}

// Payment Service
class PaymentService {
  constructor(apiClient) {
    this.api = apiClient;
  }

  async initiatePayment(paymentData) {
    return await this.api.post('/api/payments/initiate', paymentData);
  }

  async verifyPayment(moyasarPaymentId) {
    return await this.api.post(`/api/payments/verify/${moyasarPaymentId}`);
  }

  async getPaymentByOrder(orderId) {
    return await this.api.get(`/api/payments/order/${orderId}`);
  }

  async getMyPayments() {
    return await this.api.get('/api/payments/my-payments');
  }
}

// Usage Example
async function checkoutFlow() {
  try {
    // Step 1: Create Order
    const order = await orderService.createOrderFromCart(
      selectedAddressId,
      customerNotes
    );
    
    console.log('Order created:', order.data.orderNumber);
    
    // Step 2: Initiate Payment
    const payment = await paymentService.initiatePayment({
      orderId: order.data.id,
      paymentMethod: 0, // Credit Card
      cardNumber: cardNumber,
      cardHolderName: cardHolderName,
      expiryMonth: expiryMonth,
      expiryYear: expiryYear,
      cvv: cvv,
      callbackUrl: window.location.origin + '/payment-result'
    });
    
    // Step 3: Redirect to Moyasar
    if (payment.data.transactionUrl) {
      window.location.href = payment.data.transactionUrl.value;
    }
    
  } catch (error) {
    console.error('Checkout failed:', error);
    showError(error.response.data.message);
  }
}

// Handle payment result callback
async function handlePaymentCallback() {
  const urlParams = new URLSearchParams(window.location.search);
  const paymentId = urlParams.get('id');
  
  if (paymentId) {
    try {
      const payment = await paymentService.verifyPayment(paymentId);
      
      if (payment.data.status === 2) { // Paid
        showSuccess('Payment successful!');
        redirectTo(`/order-confirmation/${payment.data.orderId}`);
      } else if (payment.data.status === 3) { // Failed
        showError('Payment failed: ' + payment.data.errorMessage);
        redirectTo(`/order/${payment.data.orderId}`);
      }
    } catch (error) {
      showError('Error verifying payment');
    }
  }
}
```

---

## Additional Notes

### Important Considerations

1. **Stock Management:**
   - Stock is NOT reduced when order is created
   - Stock IS reduced only when payment is successful
   - Stock is restored if order is cancelled or refunded

2. **Order Cancellation:**
   - Users can cancel orders in `Pending`, `PaymentPending`, `Paid`, or `Processing` status
   - Cannot cancel `Shipped` or `Delivered` orders
   - Cancelling paid orders triggers automatic refund process

3. **Payment Security:**
   - Never store full card numbers in frontend
   - Only display last 4 digits from API response
   - All sensitive payment data is handled by Moyasar

4. **Webhook Handling:**
   - The `/api/payments/callback` endpoint is called by Moyasar
   - Don't call this endpoint from frontend
   - Backend automatically processes payment status updates

5. **Currency:**
   - All amounts are in SAR (Saudi Riyal)
   - Moyasar uses halalas internally (1 SAR = 100 halalas)
   - API returns amounts in SAR for convenience

### Performance Tips

1. **Order Calculation:**
   - Use `/api/orders/calculate-total` before checkout to show final amount
   - Cache calculation results until cart changes

2. **Payment Verification:**
   - After redirect from Moyasar, always verify payment status
   - Don't trust URL parameters alone
   - Implement retry logic for verification

3. **Order History:**
   - Implement pagination for order list
   - Cache order summaries
   - Reload details only when needed

### Troubleshooting

**Problem:** Order created but payment failed
- **Solution:** Order status will be `Failed`, user can retry payment or cancel order

**Problem:** Payment successful but stock not reduced
- **Solution:** Check webhook delivery, verify payment callback was processed

**Problem:** User redirected but payment status unknown
- **Solution:** Always call verify endpoint to get accurate status

**Problem:** Cart not cleared after order
- **Solution:** Cart is cleared by backend automatically after successful order creation

---

## Support & Questions

For any questions or issues:
- Backend Developer: [Your contact]
- API Documentation: [Swagger URL if available]
- Moyasar Documentation: https://docs.moyasar.com

---

**Last Updated:** January 6, 2025
**API Version:** 1.0
**Compatible with:** .NET 8, Moyasar API v1
