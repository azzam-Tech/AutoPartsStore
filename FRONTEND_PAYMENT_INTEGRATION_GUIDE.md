# ?? FRONTEND PAYMENT INTEGRATION GUIDE

## Complete Implementation Guide for All Payment Methods

---

## 1. Credit Card Payment (with 3D Secure / OTP)

### Step 1: Payment Form

```html
<!-- payment-form.html -->
<form id="credit-card-form">
    <div class="form-group">
        <label>—ﬁ„ «·»ÿ«ﬁ… / Card Number</label>
        <input type="text" id="card-number" 
               maxlength="19" 
               placeholder="1234 5678 9012 3456"
               required>
    </div>
    
    <div class="form-group">
        <label>«”„ Õ«„· «·»ÿ«ﬁ… / Cardholder Name</label>
        <input type="text" id="card-holder" 
               placeholder="John Doe"
               required>
    </div>
    
    <div class="form-row">
        <div class="form-group">
            <label> «—ÌŒ «·«‰ Â«¡ / Expiry Date</label>
            <input type="text" id="expiry-month" 
                   placeholder="MM" 
                   maxlength="2"
                   required>
            <input type="text" id="expiry-year" 
                   placeholder="YY" 
                   maxlength="2"
                   required>
        </div>
        
        <div class="form-group">
            <label>CVC</label>
            <input type="text" id="cvc" 
                   placeholder="123" 
                   maxlength="3"
                   required>
        </div>
    </div>
    
    <button type="submit" id="pay-button">
        «œ›⁄ 2,305.00 SAR / Pay 2,305.00 SAR
    </button>
</form>
```

### Step 2: JavaScript Implementation

```javascript
// payment.js
document.getElementById('credit-card-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    // Show loading
    document.getElementById('pay-button').disabled = true;
    document.getElementById('pay-button').textContent = 'Ã«—Ú «·„⁄«·Ã…...';
    
    try {
        // 1. Initiate payment
        const response = await fetch('/api/payments/initiate', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${getAuthToken()}`
            },
            body: JSON.stringify({
                orderId: getOrderId(),
                paymentMethod: 'CreditCard',
                cardNumber: document.getElementById('card-number').value.replace(/\s/g, ''),
                cardHolderName: document.getElementById('card-holder').value,
                expiryMonth: document.getElementById('expiry-month').value,
                expiryYear: document.getElementById('expiry-year').value,
                cvc: document.getElementById('cvc').value,
                callbackUrl: `${window.location.origin}/payment-result`
            })
        });
        
        const data = await response.json();
        
        if (!data.success) {
            throw new Error(data.message);
        }
        
        // 2. Redirect to Moyasar for 3D Secure / OTP
        if (data.data.transactionUrl) {
            // Moyasar will:
            // - Show 3D Secure page
            // - Send OTP to customer's phone
            // - Customer enters OTP
            // - Process payment
            // - Redirect back to callbackUrl
            window.location.href = data.data.transactionUrl;
        } else {
            // Payment completed immediately (rare)
            handlePaymentResult(data.data);
        }
    } catch (error) {
        alert('? ›‘· ›Ì »œ¡ ⁄„·Ì… «·œ›⁄: ' + error.message);
        document.getElementById('pay-button').disabled = false;
        document.getElementById('pay-button').textContent = '«œ›⁄ «·¬‰';
    }
});

function getAuthToken() {
    return localStorage.getItem('auth_token');
}

function getOrderId() {
    // Get from URL or session
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get('orderId');
}
```

### Step 3: Payment Result Page

```javascript
// payment-result.js
document.addEventListener('DOMContentLoaded', async () => {
    const urlParams = new URLSearchParams(window.location.search);
    const paymentId = urlParams.get('id');  // Moyasar payment ID from callback
    
    if (!paymentId) {
        showError('„⁄—› «·œ›⁄ „›ﬁÊœ');
        return;
    }
    
    // Show loading
    showLoading('Ã«—Ú «· Õﬁﬁ „‰ Õ«·… «·œ›⁄...');
    
    try {
        // Verify payment with backend
        const response = await fetch(`/api/payments/verify/${paymentId}`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${getAuthToken()}`
            }
        });
        
        const data = await response.json();
        
        if (!data.success) {
            throw new Error(data.message);
        }
        
        const payment = data.data;
        
        // Check payment status
        if (payment.status === 'Paid' || payment.status === 'Captured') {
            showSuccess(payment);
        } else if (payment.status === 'Failed') {
            showFailure(payment);
        } else {
            showPending(payment);
        }
    } catch (error) {
        showError('›‘· ›Ì «· Õﬁﬁ „‰ Õ«·… «·œ›⁄: ' + error.message);
    }
});

function showSuccess(payment) {
    document.getElementById('result-container').innerHTML = `
        <div class="success-message">
            <h2>?  „ «·œ›⁄ »‰Ã«Õ!</h2>
            <p>—ﬁ„ «·„⁄«„·…: ${payment.transactionReference}</p>
            <p>«·„»·€: ${payment.amount.toFixed(2)} SAR</p>
            <p>—ﬁ„ «·ÿ·»: ${payment.orderNumber}</p>
            <button onclick="window.location.href='/orders/${payment.orderId}'">
                ⁄—÷ «·ÿ·»
            </button>
        </div>
    `;
}

function showFailure(payment) {
    document.getElementById('result-container').innerHTML = `
        <div class="error-message">
            <h2>? ›‘· «·œ›⁄</h2>
            <p>${payment.errorMessage || 'ÕœÀ Œÿ√ √À‰«¡ „⁄«·Ã… «·œ›⁄'}</p>
            <button onclick="window.location.href='/checkout/${payment.orderId}'">
                Õ«Ê· „—… √Œ—Ï
            </button>
        </div>
    `;
}

function showPending(payment) {
    document.getElementById('result-container').innerHTML = `
        <div class="pending-message">
            <h2>? «·œ›⁄ ﬁÌœ «·„⁄«·Ã…</h2>
            <p>Ì—ÃÏ «·«‰ Ÿ«—...</p>
        </div>
    `;
    
    // Retry verification after 5 seconds
    setTimeout(() => location.reload(), 5000);
}

function showLoading(message) {
    document.getElementById('result-container').innerHTML = `
        <div class="loading">
            <div class="spinner"></div>
            <p>${message}</p>
        </div>
    `;
}

function showError(message) {
    document.getElementById('result-container').innerHTML = `
        <div class="error-message">
            <h2>? Œÿ√</h2>
            <p>${message}</p>
            <button onclick="window.location.href='/checkout'">
                «·⁄Êœ… ≈·Ï «·œ›⁄
            </button>
        </div>
    `;
}
```

---

## 2. Apple Pay Payment (No OTP Required)

### Step 1: Check Apple Pay Availability

```javascript
// Check if Apple Pay is available
if (window.ApplePaySession && ApplePaySession.canMakePayments()) {
    // Show Apple Pay button
    document.getElementById('apple-pay-button').style.display = 'block';
} else {
    // Hide Apple Pay button
    document.getElementById('apple-pay-button').style.display = 'none';
}
```

### Step 2: Apple Pay Button

```html
<button id="apple-pay-button" class="apple-pay-button" style="display: none;">
    <img src="/images/apple-pay-logo.svg" alt="Apple Pay">
    Pay with Apple Pay
</button>
```

### Step 3: Apple Pay Implementation

```javascript
document.getElementById('apple-pay-button').addEventListener('click', async () => {
    const orderId = getOrderId();
    const orderAmount = await getOrderAmount(orderId);
    
    // Create Apple Pay payment request
    const paymentRequest = {
        countryCode: 'SA',
        currencyCode: 'SAR',
        supportedNetworks: ['visa', 'masterCard', 'mada'],
        merchantCapabilities: ['supports3DS'],
        total: {
            label: 'Auto Parts Store',
            amount: orderAmount.toFixed(2)
        }
    };
    
    // Create Apple Pay session
    const session = new ApplePaySession(3, paymentRequest);
    
    // Handle merchant validation
    session.onvalidatemerchant = async (event) => {
        try {
            const validationResponse = await fetch('/api/payments/apple-pay/validate', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${getAuthToken()}`
                },
                body: JSON.stringify({
                    validationURL: event.validationURL
                })
            });
            
            const merchantSession = await validationResponse.json();
            session.completeMerchantValidation(merchantSession.data);
        } catch (error) {
            session.abort();
            alert('›‘· ›Ì «· Õﬁﬁ „‰ Apple Pay');
        }
    };
    
    // Handle payment authorization
    session.onpaymentauthorized = async (event) => {
        const payment = event.payment;
        
        try {
            // Send payment token to backend
            const response = await fetch('/api/payments/initiate', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${getAuthToken()}`
                },
                body: JSON.stringify({
                    orderId: orderId,
                    paymentMethod: 'ApplePay',
                    applePayToken: JSON.stringify(payment.token),  // ? Apple Pay token
                    callbackUrl: `${window.location.origin}/payment-result`
                })
            });
            
            const result = await response.json();
            
            if (result.success) {
                // Payment successful
                session.completePayment(ApplePaySession.STATUS_SUCCESS);
                
                // Redirect to success page
                window.location.href = `/payment-result?id=${result.data.paymentId}`;
            } else {
                // Payment failed
                session.completePayment(ApplePaySession.STATUS_FAILURE);
                alert('›‘· «·œ›⁄: ' + result.message);
            }
        } catch (error) {
            session.completePayment(ApplePaySession.STATUS_FAILURE);
            alert('ÕœÀ Œÿ√ √À‰«¡ „⁄«·Ã… «·œ›⁄');
        }
    };
    
    // Handle cancel
    session.oncancel = () => {
        console.log(' „ ≈·€«¡ Apple Pay');
    };
    
    // Start Apple Pay session
    session.begin();
});

async function getOrderAmount(orderId) {
    const response = await fetch(`/api/orders/${orderId}`, {
        headers: {
            'Authorization': `Bearer ${getAuthToken()}`
        }
    });
    const data = await response.json();
    return data.data.totalAmount;
}
```

### Backend Support for Apple Pay Validation (if needed)

```csharp
// PaymentsController.cs
[HttpPost("apple-pay/validate")]
[Authorize]
public async Task<IActionResult> ValidateApplePayMerchant([FromBody] ApplePayValidationRequest request)
{
    // This is optional - Moyasar handles this
    // Include only if you want to validate on your server
    
    return Success(new
    {
        // Return merchant session from Apple
    });
}
```

---

## 3. Comparison: Credit Card vs Apple Pay

| Feature | Credit Card | Apple Pay |
|---------|-------------|-----------|
| **Card Entry** | Manual input | Pre-stored |
| **3D Secure** | ? Yes (OTP) | ? No |
| **Authentication** | SMS OTP | Face ID / Touch ID |
| **Redirect** | ? Yes (Moyasar) | ? No |
| **Time** | ~30 seconds | ~5 seconds |
| **User Experience** | 5 steps | 2 steps |

---

## 4. Complete Payment Flow Diagram

```
???????????????????????????????????????????????????????????????????
? CREDIT CARD FLOW (with OTP)                                     ?
???????????????????????????????????????????????????????????????????
?                                                                  ?
?  User                  Frontend              Backend    Moyasar  ?
?   ?                       ?                     ?          ?     ?
?   ? 1. Enter card details ?                     ?          ?     ?
?   ???????????????????????>?                     ?          ?     ?
?   ?                       ? 2. POST /initiate   ?          ?     ?
?   ?                       ?????????????????????>?          ?     ?
?   ?                       ?                     ? 3. Create?     ?
?   ?                       ?                     ???????????>     ?
?   ?                       ?                     ? 4. URL   ?     ?
?   ?                       ?                     ?<?????????      ?
?   ?                       ? 5. Redirect URL     ?          ?     ?
?   ?                       ?<?????????????????????          ?     ?
?   ? 6. Redirect to Moyasar                     ?          ?     ?
?   ?<???????????????????????                     ?          ?     ?
?   ? 7. Enter OTP code     ?                     ?          ?     ?
?   ???????????????????????????????????????????????????????>?     ?
?   ?                       ?                     ? 8. Callback?   ?
?   ?                       ?                     ?<??????????     ?
?   ?                       ?                     ? 9. Update ?    ?
?   ? 10. Back to callback  ?                     ?   status  ?    ?
?   ?<???????????????????????                     ?          ?     ?
?   ?                       ? 11. Verify payment  ?          ?     ?
?   ?                       ?????????????????????>?          ?     ?
?   ? 12. Show success      ?                     ?          ?     ?
?   ?<???????????????????????                     ?          ?     ?
?                                                                  ?
???????????????????????????????????????????????????????????????????

???????????????????????????????????????????????????????????????????
? APPLE PAY FLOW (no OTP)                                          ?
???????????????????????????????????????????????????????????????????
?                                                                  ?
?  User                  Frontend              Backend    Moyasar  ?
?   ?                       ?                     ?          ?     ?
?   ? 1. Click Apple Pay    ?                     ?          ?     ?
?   ???????????????????????>?                     ?          ?     ?
?   ? 2. Face ID / Touch ID ?                     ?          ?     ?
?   ?<???????????????????????                     ?          ?     ?
?   ? 3. Approve            ?                     ?          ?     ?
?   ???????????????????????>?                     ?          ?     ?
?   ?                       ? 4. POST /initiate   ?          ?     ?
?   ?                       ?    (with token)     ?          ?     ?
?   ?                       ?????????????????????>?          ?     ?
?   ?                       ?                     ? 5. Process?    ?
?   ?                       ?                     ???????????>     ?
?   ?                       ?                     ? 6. Paid  ?     ?
?   ?                       ?                     ?<?????????      ?
?   ?                       ? 7. Success          ?          ?     ?
?   ?                       ?<?????????????????????          ?     ?
?   ? 8. Show confirmation  ?                     ?          ?     ?
?   ?<???????????????????????                     ?          ?     ?
?                                                                  ?
???????????????????????????????????????????????????????????????????
```

---

## 5. Error Handling

```javascript
// common-errors.js
const PAYMENT_ERRORS = {
    'INSUFFICIENT_FUNDS': '—’Ìœ €Ì— ﬂ«›Ú',
    'INVALID_CARD': '»ÿ«ﬁ… €Ì— ’«·Õ…',
    'EXPIRED_CARD': '»ÿ«ﬁ… „‰ ÂÌ… «·’·«ÕÌ…',
    'INCORRECT_CVC': '—„“ CVC €Ì— ’ÕÌÕ',
    'CARD_DECLINED': ' „ —›÷ «·»ÿ«ﬁ…',
    'AUTHENTICATION_FAILED': '›‘· «· Õﬁﬁ',
    'TIMEOUT': '«‰ Â  „Â·… «·⁄„·Ì…',
    'NETWORK_ERROR': 'Œÿ√ ›Ì «·« ’«·'
};

function getErrorMessage(errorCode) {
    return PAYMENT_ERRORS[errorCode] || 'ÕœÀ Œÿ√ €Ì— „ Êﬁ⁄';
}
```

---

## 6. Security Best Practices

### ? DO:
- Use HTTPS only
- Validate all inputs
- Use CSP headers
- Implement rate limiting
- Show loading states
- Handle all error cases
- Clear sensitive data after submission

### ? DON'T:
- Store card numbers
- Log sensitive data
- Allow multiple submissions
- Trust client-side validation only
- Hardcode API keys
- Skip error handling

---

## 7. Testing Cards

### Credit Cards (3D Secure)

| Card Number | Type | Result |
|-------------|------|--------|
| `4111 1111 1111 1111` | Visa | ? Success |
| `5555 5555 5555 4444` | MasterCard | ? Success |
| `4000 0000 0000 0002` | Visa | ? Declined |
| `4000 0000 0000 9995` | Visa | ? Insufficient Funds |

### Test OTP Code
- Use: `123456` (in test mode)

---

## 8. Sample Complete Integration

```html
<!DOCTYPE html>
<html>
<head>
    <title>«·œ›⁄ / Payment</title>
    <style>
        .payment-methods {
            display: flex;
            gap: 10px;
            margin-bottom: 20px;
        }
        
        .payment-method {
            border: 2px solid #ddd;
            padding: 20px;
            border-radius: 8px;
            cursor: pointer;
        }
        
        .payment-method.active {
            border-color: #007bff;
            background: #f0f8ff;
        }
        
        .apple-pay-button {
            background: black;
            color: white;
            padding: 15px;
            border: none;
            border-radius: 8px;
            cursor: pointer;
            font-size: 16px;
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>≈ „«„ «·œ›⁄ / Complete Payment</h1>
        
        <div class="order-summary">
            <p>«·„»·€ «·≈Ã„«·Ì: <strong id="order-total">2,305.00 SAR</strong></p>
        </div>
        
        <!-- Payment Method Selection -->
        <div class="payment-methods">
            <div class="payment-method active" data-method="credit-card">
                <img src="/images/credit-card.svg" alt="Credit Card">
                <p>»ÿ«ﬁ… «∆ „«‰ / Credit Card</p>
            </div>
            
            <div class="payment-method" data-method="apple-pay" id="apple-pay-option" style="display: none;">
                <img src="/images/apple-pay.svg" alt="Apple Pay">
                <p>Apple Pay</p>
            </div>
        </div>
        
        <!-- Credit Card Form -->
        <div id="credit-card-container">
            <form id="credit-card-form">
                <!-- Credit card form from above -->
            </form>
        </div>
        
        <!-- Apple Pay Button -->
        <div id="apple-pay-container" style="display: none;">
            <button id="apple-pay-button" class="apple-pay-button">
                Pay with Apple Pay
            </button>
        </div>
    </div>
    
    <script src="/js/payment.js"></script>
</body>
</html>
```

---

## 9. Complete package.json for Frontend

```json
{
  "name": "autoparts-payment",
  "version": "1.0.0",
  "dependencies": {
    "@moyasar/core": "^1.0.0"
  }
}
```

---

## 10. Final Checklist

- [ ] Implement credit card form
- [ ] Add Apple Pay button
- [ ] Handle payment result page
- [ ] Add error handling
- [ ] Test with test cards
- [ ] Test Apple Pay (on iOS device)
- [ ] Add loading states
- [ ] Add success/failure messages
- [ ] Implement retry logic
- [ ] Add payment history page

---

**Status:** ?? **Complete Frontend Integration Guide**  
**Last Updated:** January 2025
