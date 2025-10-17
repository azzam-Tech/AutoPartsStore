# ?? TAP PAYMENT GATEWAY - SETUP GUIDE

## Step 1: Add Configuration to appsettings.json

Add the following configuration to your `appsettings.json` file:

```json
{
  "TapSettings": {
    "SecretKey": "sk_test_XKokBfNWv6FIYuTMg5sLPjhJ",
    "PublishableKey": "pk_test_EtHFV4BuPQokJT6jiROls87Y",
    "BaseUrl": "https://api.tap.company/v2",
    "WebhookUrl": "https://yourstore.com/api/payments/webhook",
    "RedirectUrl": "https://yourstore.com/payment-result",
    "Enable3DSecure": true,
    "SaveCards": false,
    "StatementDescriptor": "AutoPartsStore"
  }
}
```

### Environment Variables (Production)

For production, use environment variables instead:

```bash
TAP_SECRET_KEY=sk_live_xxxxxxxxxxxxxxxx
TAP_PUBLISHABLE_KEY=pk_live_xxxxxxxxxxxxxxxx
```

---

## Step 2: Register Services in Program.cs

Add the following code to your `Program.cs`:

```csharp
using AutoPartsStore.Core.Interfaces.IServices;
using AutoPartsStore.Core.Models.Payments.Tap;
using AutoPartsStore.Infrastructure.Services;

// Add Tap settings from configuration
builder.Services.Configure<TapSettings>(
    builder.Configuration.GetSection("TapSettings"));

// Register HttpClient for Tap service
builder.Services.AddHttpClient<ITapService, TapService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

// Register Payment service
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Register repositories
builder.Services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
```

---

## Step 3: Create Database Migration

Run the following commands to create and apply the database migration:

### Option A: Using Package Manager Console

```powershell
Add-Migration RenameMoyasarToTap -Context AppDbContext
Update-Database -Context AppDbContext
```

### Option B: Using .NET CLI

```bash
dotnet ef migrations add RenameMoyasarToTap --context AppDbContext
dotnet ef database update --context AppDbContext
```

### Manual SQL Script (if needed)

If you prefer to manually update the database, run this SQL:

```sql
-- Rename column from MoyasarPaymentId to TapChargeId
EXEC sp_rename 
    'PaymentTransactions.MoyasarPaymentId', 
    'TapChargeId', 
    'COLUMN';

-- Add new CardScheme column
ALTER TABLE PaymentTransactions 
ADD CardScheme NVARCHAR(50) NULL;

-- Update existing payment statuses if needed
-- Map old statuses to new ones
UPDATE PaymentTransactions 
SET Status = 9  -- Declined
WHERE Status = 3 AND ErrorCode LIKE '%DECLINED%';

UPDATE PaymentTransactions 
SET Status = 10  -- Abandoned
WHERE Status = 3 AND ErrorCode LIKE '%ABANDONED%';
```

---

## Step 4: Test the API

### 4.1 Test Configuration

First, verify your configuration is loaded:

```csharp
// In any controller
[HttpGet("test-config")]
public IActionResult TestConfig([FromServices] IOptions<TapSettings> tapSettings)
{
    var settings = tapSettings.Value;
    return Ok(new
    {
        hasSecretKey = !string.IsNullOrEmpty(settings.SecretKey),
        hasPublishableKey = !string.IsNullOrEmpty(settings.PublishableKey),
        baseUrl = settings.BaseUrl,
        enable3DSecure = settings.Enable3DSecure
    });
}
```

### 4.2 Test Payment Initiation

Use Postman or curl to test:

```bash
POST https://localhost:7xxx/api/payments/initiate
Content-Type: application/json
Authorization: Bearer YOUR_JWT_TOKEN

{
    "orderId": 1,
    "paymentMethod": 0,
    "tapToken": "tok_xxxx",
    "firstName": "Ahmed",
    "lastName": "Ali",
    "email": "ahmed@example.com",
    "phoneNumber": "0500000000",
    "redirectUrl": "https://yourstore.com/payment-result"
}
```

---

## Step 5: Frontend Integration

### 5.1 Include Tap.js

Add this to your HTML:

```html
<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="UTF-8">
    <title>«·œ›⁄ - „ Ã— ﬁÿ⁄ «·€Ì«—</title>
    <script src="https://tap-sdk.b-cdn.net/goSell/GoSell.Web.js"></script>
</head>
<body>
    <button id="payButton" class="btn btn-primary">
        «œ›⁄ «·¬‰
    </button>
    
    <script src="payment.js"></script>
</body>
</html>
```

### 5.2 Initialize Tap Payment

Create `payment.js`:

```javascript
// Configuration
const TAP_PUBLIC_KEY = 'pk_test_EtHFV4BuPQokJT6jiROls87Y';
const API_BASE_URL = 'https://localhost:7xxx/api';

// Order information (get from your backend)
const order = {
    id: 123,
    amount: 2305.00,
    currency: 'SAR'
};

// Customer information (from your user context)
const customer = {
    firstName: '√Õ„œ',
    lastName: '⁄·Ì',
    email: 'ahmed@example.com',
    phone: {
        countryCode: '966',
        number: '500000000'
    }
};

// Initialize Tap
document.getElementById('payButton').addEventListener('click', function() {
    GoSell.config({
        containerID: "root",
        gateway: {
            publicKey: TAP_PUBLIC_KEY,
            language: "ar",
            supportedCurrencies: "SAR",
            supportedPaymentMethods: "VISA,MASTERCARD,MADA,APPLEPAY",
            saveCardOption: false,
            customerCards: {
                saveCard: false,
                autoSaveCard: false
            },
            notifications: 'standard',
            callback: handlePaymentCallback,
            onClose: handlePaymentClose,
            backgroundImg: {
                url: 'https://yourstore.com/images/payment-bg.jpg',
                opacity: '0.5'
            },
            labels: {
                cardNumber: "—ﬁ„ «·»ÿ«ﬁ…",
                expirationDate: " «—ÌŒ «·«‰ Â«¡",
                cvv: "CVV",
                cardHolder: "«”„ Õ«„· «·»ÿ«ﬁ…",
                actionButton: "«œ›⁄"
            },
            style: {
                base: {
                    color: '#535353',
                    lineHeight: '18px',
                    fontFamily: 'sans-serif',
                    fontSmoothing: 'antialiased',
                    fontSize: '16px',
                    '::placeholder': {
                        color: 'rgba(0, 0, 0, 0.26)',
                        fontSize:'15px'
                    }
                },
                invalid: {
                    color: 'red',
                    iconColor: '#fa755a '
                }
            }
        },
        customer: {
            first_name: customer.firstName,
            last_name: customer.lastName,
            email: customer.email,
            phone: customer.phone
        },
        transaction: {
            mode: 'charge',
            charge: {
                saveCard: false,
                threeDSecure: true,
                description: `ÿ·» —ﬁ„ ${order.id}`,
                statement_descriptor: 'AutoPartsStore',
                reference: {
                    transaction: `ORD-${order.id}`,
                    order: `${order.id}`
                },
                metadata: {
                    udf1: order.id.toString(),
                    udf2: customer.email
                },
                receipt: {
                    email: false,
                    sms: false
                },
                redirect: `${window.location.origin}/payment-result`,
                post: null
            },
            amount: order.amount,
            currency: order.currency
        }
    });
    
    GoSell.openLightBox();
});

// Handle payment callback
async function handlePaymentCallback(response) {
    console.log('Payment response:', response);
    
    if (response.status === 'CAPTURED') {
        // Payment successful
        showSuccessMessage(' „ «·œ›⁄ »‰Ã«Õ!');
        
        // Send to backend for verification
        try {
            const result = await fetch(`${API_BASE_URL}/payments/verify/${response.id}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${localStorage.getItem('token')}`
                }
            });
            
            const data = await result.json();
            console.log('Verification result:', data);
            
            // Redirect to success page
            window.location.href = `/order-success?orderId=${order.id}`;
        } catch (error) {
            console.error('Verification error:', error);
            showErrorMessage('ÕœÀ Œÿ√ √À‰«¡ «· Õﬁﬁ „‰ «·œ›⁄');
        }
    } else if (response.status === 'FAILED' || response.status === 'DECLINED') {
        // Payment failed
        showErrorMessage(`›‘· «·œ›⁄: ${response.response.message || 'Œÿ√ €Ì— „⁄—Ê›'}`);
    } else {
        // Other status (INITIATED, IN_PROGRESS, etc.)
        console.log('Payment status:', response.status);
    }
}

// Handle payment close
function handlePaymentClose() {
    console.log('Payment cancelled by user');
    showInfoMessage(' „ ≈·€«¡ ⁄„·Ì… «·œ›⁄');
}

// Helper functions
function showSuccessMessage(message) {
    alert(message); // Replace with your UI notification
}

function showErrorMessage(message) {
    alert(message); // Replace with your UI notification
}

function showInfoMessage(message) {
    alert(message); // Replace with your UI notification
}
```

### 5.3 Alternative: Card Tokenization

If you want to collect card details and tokenize separately:

```javascript
// Initialize Tap for tokenization only
function tokenizeCard(cardDetails) {
    return new Promise((resolve, reject) => {
        GoSell.config({
            publicKey: TAP_PUBLIC_KEY,
            transaction: {
                mode: 'token'
            }
        });
        
        GoSell.createToken(cardDetails).then(function(token) {
            resolve(token);
        }).catch(function(error) {
            reject(error);
        });
    });
}

// Usage
async function processPayment() {
    const cardDetails = {
        number: document.getElementById('cardNumber').value,
        exp_month: document.getElementById('expMonth').value,
        exp_year: document.getElementById('expYear').value,
        cvc: document.getElementById('cvc').value,
        name: document.getElementById('cardHolder').value
    };
    
    try {
        // Tokenize card
        const token = await tokenizeCard(cardDetails);
        console.log('Token:', token);
        
        // Send token to backend
        const response = await fetch(`${API_BASE_URL}/payments/initiate`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            },
            body: JSON.stringify({
                orderId: order.id,
                paymentMethod: 0, // Visa
                tapToken: token.id,
                firstName: customer.firstName,
                lastName: customer.lastName,
                email: customer.email,
                phoneNumber: customer.phone.countryCode + customer.phone.number
            })
        });
        
        const data = await response.json();
        
        if (data.success && data.data.redirectUrl) {
            // Redirect to 3D Secure
            window.location.href = data.data.redirectUrl;
        }
    } catch (error) {
        console.error('Payment error:', error);
        showErrorMessage('›‘· ›Ì „⁄«·Ã… «·œ›⁄');
    }
}
```

---

## Step 6: Apple Pay Integration

### 6.1 HTML Setup

```html
<div id="apple-pay-button" 
     style="display: none; 
            width: 200px; 
            height: 50px; 
            -apple-pay-button-style: black;
            -apple-pay-button-type: buy;">
</div>
```

### 6.2 JavaScript

```javascript
// Check if Apple Pay is available
if (window.ApplePaySession && ApplePaySession.canMakePayments()) {
    document.getElementById('apple-pay-button').style.display = 'block';
}

// Handle Apple Pay button click
document.getElementById('apple-pay-button').addEventListener('click', function() {
    const request = {
        countryCode: 'SA',
        currencyCode: 'SAR',
        supportedNetworks: ['visa', 'masterCard', 'mada'],
        merchantCapabilities: ['supports3DS'],
        total: {
            label: 'AutoPartsStore',
            amount: order.amount.toString()
        }
    };
    
    const session = new ApplePaySession(3, request);
    
    session.onvalidatemerchant = async (event) => {
        // Validate with your backend
        const response = await fetch(`${API_BASE_URL}/payments/apple-pay/validate`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            },
            body: JSON.stringify({
                validationURL: event.validationURL
            })
        });
        
        const merchantSession = await response.json();
        session.completeMerchantValidation(merchantSession);
    };
    
    session.onpaymentauthorized = async (event) => {
        const payment = event.payment;
        
        try {
            // Send to backend
            const response = await fetch(`${API_BASE_URL}/payments/initiate`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${localStorage.getItem('token')}`
                },
                body: JSON.stringify({
                    orderId: order.id,
                    paymentMethod: 3, // ApplePay
                    applePayToken: payment.token.paymentData,
                    firstName: customer.firstName,
                    lastName: customer.lastName,
                    email: customer.email,
                    phoneNumber: customer.phone.countryCode + customer.phone.number
                })
            });
            
            const data = await response.json();
            
            if (data.success) {
                session.completePayment(ApplePaySession.STATUS_SUCCESS);
                window.location.href = `/order-success?orderId=${order.id}`;
            } else {
                session.completePayment(ApplePaySession.STATUS_FAILURE);
                showErrorMessage('›‘· «·œ›⁄ ⁄»— Apple Pay');
            }
        } catch (error) {
            session.completePayment(ApplePaySession.STATUS_FAILURE);
            showErrorMessage('ÕœÀ Œÿ√ √À‰«¡ „⁄«·Ã… «·œ›⁄');
        }
    };
    
    session.begin();
});
```

---

## Step 7: Handle Payment Results

Create a payment result page:

```html
<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="UTF-8">
    <title>‰ ÌÃ… «·œ›⁄</title>
</head>
<body>
    <div id="loading">Ã«—Ì «· Õﬁﬁ „‰ «·œ›⁄...</div>
    <div id="success" style="display:none;">
        <h1>?  „ «·œ›⁄ »‰Ã«Õ!</h1>
        <p>—ﬁ„ «·ÿ·»: <span id="orderNumber"></span></p>
    </div>
    <div id="failed" style="display:none;">
        <h1>? ›‘· «·œ›⁄</h1>
        <p id="errorMessage"></p>
    </div>
    
    <script>
        // Get tap_id from URL query params
        const urlParams = new URLSearchParams(window.location.search);
        const tapId = urlParams.get('tap_id');
        
        if (tapId) {
            verifyPayment(tapId);
        } else {
            showError('„⁄·Ê„«  «·œ›⁄ €Ì— „ÊÃÊœ…');
        }
        
        async function verifyPayment(chargeId) {
            try {
                const response = await fetch(`${API_BASE_URL}/payments/verify/${chargeId}`, {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${localStorage.getItem('token')}`
                    }
                });
                
                const data = await response.json();
                
                document.getElementById('loading').style.display = 'none';
                
                if (data.success && data.data.status === 2) { // Paid
                    document.getElementById('success').style.display = 'block';
                    document.getElementById('orderNumber').textContent = data.data.orderNumber;
                    
                    // Redirect after 3 seconds
                    setTimeout(() => {
                        window.location.href = `/orders/${data.data.orderId}`;
                    }, 3000);
                } else {
                    showError(data.data.errorMessage || '›‘· «·œ›⁄');
                }
            } catch (error) {
                showError('ÕœÀ Œÿ√ √À‰«¡ «· Õﬁﬁ „‰ «·œ›⁄');
            }
        }
        
        function showError(message) {
            document.getElementById('loading').style.display = 'none';
            document.getElementById('failed').style.display = 'block';
            document.getElementById('errorMessage').textContent = message;
        }
    </script>
</body>
</html>
```

---

## Step 8: Testing

### Test Cards

Use these test cards in test mode:

| Card Number | Type | 3D Secure | Result |
|-------------|------|-----------|--------|
| `4111 1111 1111 1111` | Visa | Yes | Success |
| `5555 5555 5555 4444` | MasterCard | Yes | Success |
| `4000 0000 0000 0002` | Visa | Yes | Declined |
| `4000 0000 0000 9995` | Visa | Yes | Insufficient Funds |

**Test Data:**
- CVV: Any 3 digits (e.g., `123`)
- Expiry: Any future date (e.g., `12/25`)
- 3D Secure OTP: `123456`

---

## Step 9: Go Live

### 9.1 Get Production Keys

1. Login to Tap Dashboard: https://dashboard.tap.company
2. Go to Settings ? API Keys
3. Copy your **Live** keys

### 9.2 Update Configuration

```json
{
  "TapSettings": {
    "SecretKey": "sk_live_xxxxxxxxxxxxxxxx",
    "PublishableKey": "pk_live_xxxxxxxxxxxxxxxx",
    "BaseUrl": "https://api.tap.company/v2",
    "WebhookUrl": "https://yourstore.com/api/payments/webhook",
    "RedirectUrl": "https://yourstore.com/payment-result",
    "Enable3DSecure": true,
    "SaveCards": false,
    "StatementDescriptor": "AutoPartsStore"
  }
}
```

### 9.3 Setup Webhook

1. Login to Tap Dashboard
2. Go to Settings ? Webhooks
3. Add webhook URL: `https://yourstore.com/api/payments/webhook`
4. Select events: `charge.created`, `charge.updated`, `charge.captured`, `charge.failed`

---

## Troubleshooting

### Issue: "Authentication failed"

**Solution:** Check that your Secret Key is correct and starts with `sk_test_` or `sk_live_`

### Issue: "Invalid token"

**Solution:** Ensure Tap.js is creating tokens with the correct Publishable Key

### Issue: "3D Secure redirect not working"

**Solution:** Check that `redirectUrl` in TapSettings points to a valid page

### Issue: "Webhook not receiving events"

**Solution:** 
1. Ensure webhook URL is publicly accessible (not localhost)
2. Check Tap Dashboard ? Webhooks ? Logs for errors
3. Verify `AllowAnonymous` attribute is on webhook endpoint

---

## Security Checklist

- [ ] Never expose Secret Key in frontend code
- [ ] Always use HTTPS in production
- [ ] Validate webhook signatures (if Tap provides them)
- [ ] Use environment variables for sensitive data
- [ ] Enable 3D Secure for all card payments
- [ ] Log all payment attempts for audit
- [ ] Implement rate limiting on payment endpoints
- [ ] Validate payment amounts match order totals

---

## Support

- ?? **Tap Documentation:** https://developers.tap.company/docs
- ?? **API Reference:** https://developers.tap.company/reference
- ?? **Support:** support@tap.company
- ?? **Phone:** +965 2220 4440

---

**Setup Complete! ??**

Your Tap Payment Gateway integration is now ready to process payments.
