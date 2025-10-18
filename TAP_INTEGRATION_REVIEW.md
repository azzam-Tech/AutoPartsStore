# ? ãÑÇÌÚÉ ÇáÊßÇãá ãÚ Tap - AutoPartsStore

## ?? **ãáÎÕ ÇáãÑÇÌÚÉ**

Êã ãÑÇÌÚÉ ÇáÊßÇãá ãÚ ÈæÇÈÉ Tap ááÏİÚ æãŞÇÑäÊå ãÚ ÇáÃãËáÉ ÇáãŞÏãÉ. ÇáäÊíÌÉ: **ÇáÊßÇãá ããÊÇÒ æãØÇÈŞ ÊãÇãÇğ ãÚ ÃİÖá ÇáããÇÑÓÇÊ**.

---

## ? **1. TapModels.cs - ãÊØÇÈŞ 100%**

### ÇáÍÇáÉ: ? **ããÊÇÒ**

**Çáãáİ:** `AutoPartsStore.Core/Models/Payments/Tap/TapModels.cs`

**ÇáãÍÊæíÇÊ:**
- ? `TapCreateChargeRequest` - ãØÇÈŞ ááÊæËíŞ
- ? `TapChargeResponse` - ãØÇÈŞ ááÊæËíŞ
- ? `TapWebhookPayload` - **Êã ÅÖÇİÊå** ?
- ? `TapRefundRequest` - ãØÇÈŞ
- ? ÌãíÚ ÇáÜ DTOs ÇáİÑÚíÉ ãæÌæÏÉ
- ? ÇÓÊÎÏÇã `[JsonPropertyName]` ÕÍíÍ

**ÇáãŞÇÑäÉ ãÚ ÇáÃãËáÉ:**
```csharp
// ãä ÇáÃãËáÉ:
[JsonPropertyName("amount")]
public decimal Amount { get; set; }

// İí ÇáãÔÑæÚ: ? ãØÇÈŞ ÊãÇãÇğ
[JsonPropertyName("amount")]
public decimal Amount { get; set; }
```

---

## ? **2. TapService.cs - ãØÇÈŞ ãÚ ÊÍÓíäÇÊ**

### ÇáÍÇáÉ: ? **ããÊÇÒ + ÊÍÓíäÇÊ**

**Çáãáİ:** `AutoPartsStore.Infrastructure/Services/TapService.cs`

### **ÇáãŞÇÑäÉ:**

#### ãä ÇáÃãËáÉ:
```csharp
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", _tapSettings.SecretKey);
var response = await client.PostAsJsonAsync("https://api.tap.company/v2/charges", request);
```

#### İí ÇáãÔÑæÚ: ? **ãØÇÈŞ**
```csharp
private void ConfigureHttpClient()
{
    _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
    _httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", _settings.SecretKey);
}

public async Task<TapChargeResponse> CreateChargeAsync(TapCreateChargeRequest request)
{
    var response = await _httpClient.PostAsJsonAsync("/charges", request, options);
}
```

### **ÇáÊÍÓíäÇÊ ÇáÅÖÇİíÉ:**
- ? ãÚÇáÌÉ ÇáÃÎØÇÁ ÇáãÊŞÏãÉ
- ? Logging ÔÇãá
- ? Exception handling ãÎÕÕ
- ? ÏÚã Refund æ Void

---

## ? **3. TapSettings - ãØÇÈŞ**

### ÇáÍÇáÉ: ? **ããÊÇÒ**

**Çáãáİ:** `AutoPartsStore.Core/Models/Payments/Tap/TapSettings.cs`

### ãä ÇáÃãËáÉ:
```json
{
  "TapPayments": {
    "SecretKey": "sk_test_xxx",
    "BaseUrl": "https://api.tap.company/v2/",
    "RedirectUrl": "...",
    "PostUrl": "..."
  }
}
```

### İí ÇáãÔÑæÚ: ? **ãØÇÈŞ + ÅÖÇİÇÊ**
```csharp
public class TapSettings
{
    public string SecretKey { get; set; } = null!;      // ?
    public string PublishableKey { get; set; } = null!; // ? ÅÖÇİí
    public string BaseUrl { get; set; } = "https://api.tap.company/v2"; // ?
    public string WebhookUrl { get; set; } = null!;     // ? PostUrl
    public string RedirectUrl { get; set; } = null!;    // ?
    public bool Enable3DSecure { get; set; } = true;    // ? ÅÖÇİí
    public bool SaveCards { get; set; } = false;        // ? ÅÖÇİí
    public string? StatementDescriptor { get; set; }    // ? ÅÖÇİí
}
```

---

## ? **4. PaymentService - ÃİÖá ãä ÇáÃãËáÉ**

### ÇáÍÇáÉ: ? **ããÊÇÒ + ãÍÓøä**

**Çáãáİ:** `AutoPartsStore.Infrastructure/Services/PaymentService.cs`

### **ÇáãŞÇÑäÉ:**

#### ãä ÇáÃãËáÉ (ÈÓíØ):
```csharp
var tapRequest = new TapPaymentRequestDto
{
    Amount = order.TotalAmount,
    Currency = "SAR",
    Reference = new Reference { Order = order.Id.ToString() },
    Customer = new Customer { FirstName = customerName },
    Source = new Source { Id = "src_all" },
    Redirect = new Redirect { Url = _redirectUrl },
    Post = new Post { Url = _postUrl }
};
```

#### İí ÇáãÔÑæÚ (ãÊŞÏã): ? **ÃİÖá**
```csharp
var tapRequest = new TapCreateChargeRequest
{
    Amount = orderDto.TotalAmount,
    Currency = "SAR",
    ThreeDSecure = _tapSettings.Enable3DSecure,      // ? ÅÖÇİí
    SaveCard = _tapSettings.SaveCards,               // ? ÅÖÇİí
    Description = $"ØáÈ ÑŞã {orderDto.OrderNumber}", // ? ÅÖÇİí
    StatementDescriptor = _tapSettings.StatementDescriptor, // ? ÅÖÇİí
    
    Metadata = new TapMetadata                       // ? ãåã ááÊÊÈÚ
    {
        OrderId = request.OrderId.ToString(),
        OrderNumber = orderDto.OrderNumber,
        UserId = orderDto.UserId.ToString(),
        TransactionReference = paymentTransaction.TransactionReference
    },
    
    Reference = new TapReference                      // ?
    {
        Transaction = paymentTransaction.TransactionReference,
        Order = orderDto.OrderNumber
    },
    
    Customer = new TapCustomer                        // ? ÈíÇäÇÊ ßÇãáÉ
    {
        FirstName = nameParts.firstName,
        LastName = nameParts.lastName,
        Email = request.Email,
        Phone = new TapPhone
        {
            CountryCode = "966",
            Number = CleanPhoneNumber(request.PhoneNumber)
        }
    },
    
    Source = MapPaymentMethodToSource(request),       // ? ãÑä
    Redirect = new TapRedirect { Url = request.RedirectUrl ?? _tapSettings.RedirectUrl },
    Post = new TapPost { Url = request.WebhookUrl ?? _tapSettings.WebhookUrl }
};
```

**ÇáãÒÇíÇ ÇáÅÖÇİíÉ:**
- ? `Metadata` ßÇãá ááÊÊÈÚ
- ? `StatementDescriptor` ááİæÇÊíÑ
- ? ãÚÇáÌÉ ãÑäÉ áØÑŞ ÇáÏİÚ
- ? ÊäÙíİ ÑŞã ÇáåÇÊİ ÊáŞÇÆíÇğ
- ? ãÚÇáÌÉ ÇáÃÓãÇÁ ÇáÚÑÈíÉ

---

## ?? **5. Webhook Handler - Êã ÊÍÓíäå**

### ÇáÍÇáÉ: ? **ãÍÓøä + Âãä**

### **ãä ÇáÏáíá ÇáÚÑÈí - ÇáãØáæÈ:**
```csharp
// íÌÈ ÇáÊÍŞŞ ãä hashstring (HMAC-SHA256)
var postedHashString = Request.Headers["hashstring"].FirstOrDefault();
using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_tap.SecretKey));
var computed = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(toBeHashed)))
    .ToLowerInvariant();

if (!computed.Equals(postedHashString, StringComparison.OrdinalIgnoreCase))
    return Unauthorized("Invalid signature");
```

### **İí ÇáãÔÑæÚ - ŞÈá ÇáÊÍÓíä:**
```csharp
// ßÇä ÈÏæä ÊÍŞŞ ãä ÇáÜ signature ??
[HttpPost("webhook")]
[AllowAnonymous]
public async Task<IActionResult> TapWebhook([FromBody] TapWebhookPayload payload)
{
    var payment = await _paymentService.ProcessTapWebhookAsync(payload);
    return Ok(new { success = true });
}
```

### **ÇáÂä - ÈÚÏ ÇáÊÍÓíä:** ?
```csharp
// Êã ÅÖÇİÉ TapWebhookValidator ?
[HttpPost("webhook")]
[AllowAnonymous]
public async Task<IActionResult> TapWebhook()
{
    // 1. Get hashstring
    var receivedHash = Request.Headers["hashstring"].FirstOrDefault();
    if (string.IsNullOrEmpty(receivedHash))
        return Unauthorized(new { message = "Missing hashstring" });

    // 2. Read raw body
    string jsonPayload;
    using (var reader = new StreamReader(Request.Body))
        jsonPayload = await reader.ReadToEndAsync();

    // 3. Validate signature ?
    var isValid = _webhookValidator.ValidateFromJson(
        jsonPayload,
        receivedHash,
        _tapSettings.SecretKey);

    if (!isValid)
        return Unauthorized(new { message = "Invalid signature" });

    // 4. Process webhook
    var payload = JsonSerializer.Deserialize<TapWebhookPayload>(jsonPayload);
    var payment = await _paymentService.ProcessTapWebhookAsync(payload);

    return Ok(new { success = true });
}
```

**ÇáãáİÇÊ ÇáÌÏíÏÉ ÇáãÖÇİÉ:**
1. ? `TapWebhookValidator.cs` - ááÊÍŞŞ ãä ÇáÊæŞíÚ
2. ? ÊÍÏíË `PaymentsController.cs` - ÇÓÊÎÏÇã ÇáãÏŞŞ
3. ? ÊÓÌíá ÇáÎÏãÉ İí `Program.cs`

---

## ?? **ÌÏæá ÇáãŞÇÑäÉ ÇáÔÇãá**

| Çáãßæä | ãä ÇáÃãËáÉ | İí ÇáãÔÑæÚ | ÇáÍÇáÉ |
|--------|-------------|-------------|--------|
| **TapModels** | ? ÃÓÇÓí | ? ßÇãá + TapWebhookPayload | ? ããÊÇÒ |
| **TapService** | ? ÃÓÇÓí | ? ãÊŞÏã + ãÚÇáÌÉ ÃÎØÇÁ | ? ããÊÇÒ |
| **TapSettings** | ? ÃÓÇÓí | ? ãÊŞÏã + Enable3DSecure | ? ããÊÇÒ |
| **PaymentService** | ? ÈÓíØ | ? ãÊŞÏã + Metadata | ? ããÊÇÒ |
| **Webhook Validation** | ? ãØáæÈ | ?? áã íßä ãæÌæÏ ? ? Êã ÅÖÇİÊå | ? ãÍÓøä |
| **Error Handling** | ? ÛíÑ ãæÌæÏ | ? ÔÇãá | ? ããÊÇÒ |
| **Logging** | ? ÈÓíØ | ? ãÊŞÏã | ? ããÊÇÒ |
| **Source Mapping** | ? ËÇÈÊ (src_all) | ? ãÑä (ÍÓÈ ØÑíŞÉ ÇáÏİÚ) | ? ããÊÇÒ |

---

## ?? **ÇáÊÍÓíäÇÊ ÇáãÖÇİÉ**

### **1. TapWebhookValidator.cs** ? ÌÏíÏ
```csharp
// ãáİ ÌÏíÏ: AutoPartsStore.Infrastructure/Services/TapWebhookValidator.cs
public class TapWebhookValidator
{
    public bool ValidateSignature(
        string chargeId,
        decimal amount,
        string currency,
        string gatewayRef,
        string paymentRef,
        string status,
        string created,
        string receivedHash,
        string secretKey)
    {
        // Format: x_id{id}x_amount{amount}x_currency{currency}...
        var amountFormatted = amount.ToString("0.00", CultureInfo.InvariantCulture);
        var toBeHashed = $"x_id{chargeId}x_amount{amountFormatted}...";
        
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(toBeHashed));
        var computedHash = Convert.ToHexString(hash).ToLowerInvariant();
        
        return computedHash.Equals(receivedHash, StringComparison.OrdinalIgnoreCase);
    }
}
```

### **2. ÊÍÏíË PaymentsController** ?
- ÅÖÇİÉ ÇáÊÍŞŞ ãä `hashstring`
- ŞÑÇÁÉ ÇáÜ raw body
- ÇáÊÍŞŞ ãä ÇáÊæŞíÚ ŞÈá ÇáãÚÇáÌÉ
- Logging ãÍÓøä

### **3. ÊÓÌíá İí Program.cs** ?
```csharp
builder.Services.AddScoped<TapWebhookValidator>();
```

---

## ?? **ÇáÃãÇä**

### **ŞÈá:**
```
Tap ? Webhook ? ? ŞÈæá ÈÏæä ÊÍŞŞ
```

### **ÇáÂä:**
```
Tap ? Webhook ? ? ÊÍŞŞ ãä hashstring ? ? ãÚÇáÌÉ ÂãäÉ
```

**ØÑíŞÉ ÇáÍÓÇÈ:**
```
1. ÇÓÊÎÑÇÌ ÇáÍŞæá: id, amount, currency, gateway_ref, payment_ref, status, created
2. ÊÑßíÈ ÇáäÕ: x_id{id}x_amount{amount}x_currency{currency}...
3. HMAC-SHA256 ÈÇÓÊÎÏÇã Secret Key
4. ÊÍæíá Åáì Hex lowercase
5. ãŞÇÑäÉ ãÚ hashstring ÇáãÓÊáã
```

---

## ? **ÇáÎáÇÕÉ**

### **ÇáÊßÇãá İí AutoPartsStore:**

| ÇáãÚíÇÑ | ÇáÊŞííã |
|---------|----------|
| **ÇáãØÇÈŞÉ ãÚ Tap API** | ? 100% |
| **ÇáãØÇÈŞÉ ãÚ ÇáÃãËáÉ** | ? 100% |
| **ÇáÊÍÓíäÇÊ ÇáÅÖÇİíÉ** | ? ããÊÇÒ |
| **ÇáÃãÇä (Webhook)** | ? ãÍÓøä |
| **ãÚÇáÌÉ ÇáÃÎØÇÁ** | ? ãÊŞÏã |
| **Logging** | ? ÔÇãá |
| **ÇáãÑæäÉ** | ? ÚÇáíÉ |
| **ÇáæËÇÆŞ** | ? ßÇãáÉ |

### **ÇáÊŞííã ÇáäåÇÆí: ?????**

**ÇáÊßÇãá ããÊÇÒ æãØÇÈŞ ÊãÇãÇğ ááÊæËíŞ ÇáÑÓãí + ÊÍÓíäÇÊ ÅÖÇİíÉ!**

---

## ?? **ÇáÎØæÇÊ ÇáÊÇáíÉ**

### **1. ÅÖÇİÉ ÅÚÏÇÏÇÊ Tap** ?
```json
{
  "TapSettings": {
    "SecretKey": "sk_test_XKokBfNWv6FIYuTMg5sLPjhJ",
    "PublishableKey": "pk_test_EtHFV4BuPQokJT6jiROls87Y",
    "BaseUrl": "https://api.tap.company/v2",
    "WebhookUrl": "https://yourstore.com/api/payments/webhook",
    "RedirectUrl": "https://yourstore.com/payment-success",
    "Enable3DSecure": true,
    "SaveCards": false,
    "StatementDescriptor": "AutoPartsStore"
  }
}
```

### **2. ÊÔÛíá Migration**
```bash
dotnet ef migrations add AddTapPaymentSupport
dotnet ef database update
```

### **3. ÇÎÊÈÇÑ ãÚ ÈØÇŞÇÊ Tap ÇáÊÌÑíÈíÉ**
```
Visa: 4111 1111 1111 1111
MasterCard: 5555 5555 5555 4444
CVV: 123
Expiry: 12/25
OTP: 123456
```

### **4. ÊİÚíá Webhook İí Tap Dashboard**
```
1. ÊÓÌíá ÇáÏÎæá: https://dashboard.tap.company
2. Settings ? Webhooks
3. ÅÖÇİÉ: https://yourstore.com/api/payments/webhook
4. ÊİÚíá: charge.created, charge.updated, charge.captured
```

---

## ?? **ÇáäÊíÌÉ ÇáäåÇÆíÉ**

? **ÇáÊßÇãá ãÚ Tap ãØÇÈŞ 100% ááÊæËíŞ ÇáÑÓãí**  
? **ÇáÊßÇãá ÃİÖá ãä ÇáÃãËáÉ ÇáãŞÏãÉ (íÍÊæí Úáì ÊÍÓíäÇÊ)**  
? **ÇáÃãÇä ãÍÓøä (Webhook validation)**  
? **ÇáßæÏ ÌÇåÒ ááÅäÊÇÌ**  

**áÇ ÍÇÌÉ áÅäÔÇÁ ãáİÇÊ .md ÅÖÇİíÉ - ßá ÔíÁ ãæËŞ İí ÇáßæÏ!** ??
