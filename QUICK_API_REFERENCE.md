# AutoPartsStore - Quick API Reference

## ?? Quick Start

### Authentication
All endpoints require JWT Bearer token in header:
```
Authorization: Bearer {your_jwt_token}
```

### Base URL
```
https://yourdomain.com/api
```

---

## ?? Order Creation Flow (3 Steps)

### Step 1: Create Order
```http
POST /api/orders/from-cart
Content-Type: application/json
Authorization: Bearer {token}

{
  "shippingAddressId": 1,
  "customerNotes": "Optional notes"
}
```

**Response:** Returns complete order object with `orderId`

### Step 2: Initiate Payment
```http
POST /api/payments/initiate
Content-Type: application/json
Authorization: Bearer {token}

{
  "orderId": 123,
  "paymentMethod": 0,
  "cardNumber": "4111111111111111",
  "cardHolderName": "Name",
  "expiryMonth": "12",
  "expiryYear": "2025",
  "cvv": "123",
  "callbackUrl": "https://yoursite.com/payment-result"
}
```

**Response:** Returns Moyasar payment details with redirect URL

### Step 3: Verify Payment
```http
POST /api/payments/verify/{moyasarPaymentId}
Authorization: Bearer {token}
```

**Response:** Returns payment status

---

## ?? Order Status Codes

| Code | Status | Description |
|------|--------|-------------|
| 0 | Pending | Order created, awaiting payment |
| 1 | PaymentPending | Payment initiated |
| 2 | Paid | Payment successful ? |
| 3 | Processing | Order being prepared |
| 4 | Shipped | Order dispatched |
| 5 | Delivered | Order completed ? |
| 6 | Cancelled | Order cancelled ? |
| 7 | Refunded | Payment refunded |
| 8 | Failed | Payment failed ? |

---

## ?? Payment Method Codes

| Code | Method | Notes |
|------|--------|-------|
| 0 | CreditCard | Visa/MasterCard |
| 1 | Mada | Saudi local cards |
| 2 | ApplePay | Requires token |
| 3 | STCPay | Saudi mobile wallet |
| 4 | Tabby | Buy now, pay later |
| 5 | Tamara | Buy now, pay later |

---

## ?? Order Calculation

```javascript
SubTotal = Sum of all items
DiscountAmount = Sum of all discounts
TaxAmount = (SubTotal - DiscountAmount) ◊ 0.15  // 15% VAT
ShippingCost = 25.00 SAR
TotalAmount = (SubTotal - DiscountAmount) + TaxAmount + ShippingCost
```

---

## ?? Essential Endpoints

### User Order Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/orders/from-cart` | Create order from cart |
| POST | `/api/orders` | Create order directly |
| GET | `/api/orders/my-orders` | Get user's orders |
| GET | `/api/orders/{id}` | Get order details |
| PATCH | `/api/orders/{id}/cancel` | Cancel order |
| POST | `/api/orders/calculate-total` | Calculate order total |

### User Payment Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/payments/initiate` | Start payment |
| POST | `/api/payments/verify/{id}` | Verify payment |
| GET | `/api/payments/my-payments` | Get user's payments |
| GET | `/api/payments/order/{orderId}` | Get payment by order |

### Admin Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/orders` | List all orders (with filters) |
| PATCH | `/api/orders/{id}/status` | Update order status |
| PATCH | `/api/orders/{id}/tracking` | Update tracking number |
| POST | `/api/payments/{id}/refund` | Process refund |
| GET | `/api/orders/statistics` | Order statistics |
| GET | `/api/payments/summary` | Payment summary |

---

## ?? Test Cards (Test Mode)

| Type | Number | Result |
|------|--------|--------|
| Visa | 4111111111111111 | ? Success |
| MasterCard | 5123450000000008 | ? Success |
| Visa Declined | 4000000000000002 | ? Declined |
| Mada | 5297410000000000 | ? Success |

**CVV:** Any 3 digits
**Expiry:** Any future date

---

## ?? Common Errors

### Empty Cart
```json
{
  "success": false,
  "message": "”·… «· ”Êﬁ ›«—€…. ·« Ì„ﬂ‰ ≈‰‘«¡ ÿ·»."
}
```
**Solution:** Ensure cart has items before creating order

### Out of Stock
```json
{
  "success": false,
  "message": "»⁄÷ «·„‰ Ã«  €Ì— „ Ê›—… »«·ﬂ„Ì… «·„ÿ·Ê»….",
  "errors": ["Product X - Available: 1, Requested: 2"]
}
```
**Solution:** Update cart quantities

### Payment Failed
```json
{
  "success": false,
  "message": "›‘·  ⁄„·Ì… «·œ›⁄.",
  "data": {
    "paymentStatus": "Failed",
    "errorMessage": "Insufficient funds"
  }
}
```
**Solution:** Ask user to try different payment method

### Unauthorized
```json
{
  "success": false,
  "message": "€Ì— „’—Õ. Ì—ÃÏ  ”ÃÌ· «·œŒÊ·."
}
```
**Solution:** Token expired or invalid, redirect to login

---

## ?? Response Format

### Success Response
```json
{
  "success": true,
  "message": "Operation successful message",
  "data": { /* response data */ },
  "errors": []
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error message in Arabic",
  "data": null,
  "errors": ["Error detail 1", "Error detail 2"]
}
```

---

## ?? Complete Checkout Flow

```javascript
// 1. Validate cart
if (cart.items.length === 0) {
  showError('Cart is empty');
  return;
}

// 2. Create order
const order = await createOrder(addressId, notes);

// 3. Initiate payment
const payment = await initiatePayment({
  orderId: order.data.id,
  paymentMethod: selectedMethod,
  // ... card details
  callbackUrl: window.location.origin + '/payment-result'
});

// 4. Redirect to Moyasar
window.location.href = payment.data.transactionUrl.value;

// 5. After redirect back, verify payment
const urlParams = new URLSearchParams(window.location.search);
const paymentId = urlParams.get('id');

const paymentStatus = await verifyPayment(paymentId);

// 6. Show result
if (paymentStatus.data.status === 2) {
  showSuccess('Payment successful!');
  redirectTo('/order-confirmation');
} else {
  showError('Payment failed');
}
```

---

## ?? Frontend Checklist

- [ ] Implement order creation from cart
- [ ] Add payment method selection UI
- [ ] Handle Moyasar redirect
- [ ] Process payment callback
- [ ] Display order history
- [ ] Show order details with items
- [ ] Implement order tracking
- [ ] Add order cancellation
- [ ] Display payment history
- [ ] Handle all error cases
- [ ] Add loading states
- [ ] Implement retry logic

---

## ?? Security Notes

1. ? Never store full card numbers
2. ? All payment data goes through Moyasar
3. ? Always verify payment status from backend
4. ? Use HTTPS for all requests
5. ? Store JWT token securely (httpOnly cookie preferred)

---

## ?? Need Help?

- **Full Documentation:** `FRONTEND_ORDER_PAYMENT_GUIDE.md`
- **Moyasar Docs:** https://docs.moyasar.com
- **Contact:** [Your backend developer contact]

---

**Last Updated:** January 6, 2025
