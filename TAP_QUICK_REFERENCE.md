# ?? TAP PAYMENT - QUICK REFERENCE CARD

## ? Quick Start (3 Steps)

### 1. Configuration (appsettings.json)
```json
{
  "TapSettings": {
    "SecretKey": "sk_test_XKokBfNWv6FIYuTMg5sLPjhJ",
    "PublishableKey": "pk_test_EtHFV4BuPQokJT6jiROls87Y",
    "WebhookUrl": "https://yourstore.com/api/payments/webhook",
    "RedirectUrl": "https://yourstore.com/payment-result"
  }
}
```

### 2. Program.cs
```csharp
builder.Services.Configure<TapSettings>(
    builder.Configuration.GetSection("TapSettings"));
builder.Services.AddHttpClient<ITapService, TapService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
```

### 3. Database Migration
```bash
dotnet ef migrations add RenameMoyasarToTap
dotnet ef database update
```

---

## ?? API Endpoints

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/payments/initiate` | POST | ? | Create payment |
| `/api/payments/webhook` | POST | ? | Tap webhook |
| `/api/payments/verify/{id}` | POST | ? | Verify payment |
| `/api/payments/{id}` | GET | ? | Get payment |
| `/api/payments/order/{id}` | GET | ? | Get by order |
| `/api/payments/{id}/refund` | POST | Admin | Refund |

---

## ?? Payment Methods

| Method | Code | Token Required |
|--------|------|----------------|
| Visa | `0` | `tok_xxxx` |
| MasterCard | `1` | `tok_xxxx` |
| Mada | `2` | `tok_xxxx` |
| Apple Pay | `3` | Apple Pay token |
| Tabby | `4` | `tabby` |

---

## ?? Payment Statuses

| Status | Code | Description |
|--------|------|-------------|
| Initiated | `0` | Created |
| Pending | `1` | Processing |
| Paid | `2` | ? Success |
| Failed | `3` | ? Failed |
| Captured | `5` | ? Success |
| Refunded | `6` | Refunded |
| Declined | `9` | ? Declined |

---

## ?? Frontend Integration

### Include Tap.js
```html
<script src="https://tap-sdk.b-cdn.net/goSell/GoSell.Web.js"></script>
```

### Initialize Payment
```javascript
GoSell.config({
    publicKey: 'pk_test_xxx',
    language: 'ar',
    supportedPaymentMethods: 'VISA,MASTERCARD,MADA,APPLEPAY',
    callback: (response) => {
        initiatePayment(response.token);
    },
    customer: {
        first_name: 'Ahmed',
        last_name: 'Ali',
        email: 'ahmed@example.com',
        phone: { country_code: '966', number: '500000000' }
    },
    transaction: {
        amount: 2305.00,
        currency: 'SAR'
    }
});

GoSell.openLightBox();
```

### Send to Backend
```javascript
async function initiatePayment(token) {
    const response = await fetch('/api/payments/initiate', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify({
            orderId: 123,
            paymentMethod: 0,
            tapToken: token,
            firstName: 'Ahmed',
            lastName: 'Ali',
            email: 'ahmed@example.com',
            phoneNumber: '0500000000'
        })
    });
    
    const data = await response.json();
    if (data.success) {
        window.location.href = data.data.redirectUrl; // 3D Secure
    }
}
```

---

## ?? Test Cards

| Card Number | Type | Result |
|-------------|------|--------|
| `4111 1111 1111 1111` | Visa | ? Success |
| `5555 5555 5555 4444` | MasterCard | ? Success |
| `4000 0000 0000 0002` | Visa | ? Declined |

**Test Data:**
- CVV: `123`
- Expiry: `12/25`
- 3D Secure OTP: `123456`

---

## ?? Common Errors

| Error | Solution |
|-------|----------|
| `Authentication failed` | Check Secret Key |
| `Invalid token` | Check Publishable Key |
| `3D Secure not working` | Check RedirectUrl |
| `Webhook not called` | Check URL is public (not localhost) |

---

## ?? Status Mapping

### Tap ? Internal

```csharp
return tapStatus?.ToUpper() switch
{
    "INITIATED" => PaymentStatus.Initiated,
    "IN_PROGRESS" => PaymentStatus.Pending,
    "CAPTURED" => PaymentStatus.Captured,
    "FAILED" => PaymentStatus.Failed,
    "DECLINED" => PaymentStatus.Declined,
    "CANCELLED" => PaymentStatus.Cancelled,
    "ABANDONED" => PaymentStatus.Abandoned,
    "VOID" => PaymentStatus.Voided,
    _ => PaymentStatus.Pending
};
```

---

## ?? Webhook Handler

```csharp
[HttpPost("webhook")]
[AllowAnonymous]
public async Task<IActionResult> PaymentWebhook([FromBody] ProcessPaymentWebhookRequest request)
{
    try
    {
        var payment = await _paymentService.ProcessPaymentWebhookAsync(request.ChargeId);
        return Success(payment);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Webhook error");
        return Ok(new { success = false, error = ex.Message });
    }
}
```

---

## ?? Security Checklist

- [ ] Never expose Secret Key in frontend
- [ ] Always use HTTPS in production
- [ ] Use environment variables for keys
- [ ] Enable 3D Secure for all card payments
- [ ] Implement rate limiting
- [ ] Validate amounts match orders
- [ ] Log all payment attempts

---

## ?? Support

- ?? **Docs:** https://developers.tap.company/docs
- ?? **API:** https://developers.tap.company/reference
- ?? **Email:** support@tap.company
- ?? **Phone:** +965 2220 4440

---

## ?? Production Checklist

- [ ] Update to live keys (`sk_live_`, `pk_live_`)
- [ ] Configure webhook URL in Tap Dashboard
- [ ] Test with real cards (small amounts)
- [ ] Monitor webhook logs
- [ ] Set up error alerting
- [ ] Document API keys location

---

**Quick Reference Version:** 1.0  
**Last Updated:** January 2025  
**Status:** ? Ready for Production
