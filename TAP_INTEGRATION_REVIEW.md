# ? ������ ������� �� Tap - AutoPartsStore

## ?? **���� ��������**

�� ������ ������� �� ����� Tap ����� �������� �� ������� �������. �������: **������� ����� ������ ������ �� ���� ���������**.

---

## ? **1. TapModels.cs - ������ 100%**

### ������: ? **�����**

**�����:** `AutoPartsStore.Core/Models/Payments/Tap/TapModels.cs`

**���������:**
- ? `TapCreateChargeRequest` - ����� �������
- ? `TapChargeResponse` - ����� �������
- ? `TapWebhookPayload` - **�� ������** ?
- ? `TapRefundRequest` - �����
- ? ���� ��� DTOs ������� ������
- ? ������� `[JsonPropertyName]` ����

**�������� �� �������:**
```csharp
// �� �������:
[JsonPropertyName("amount")]
public decimal Amount { get; set; }

// �� �������: ? ����� ������
[JsonPropertyName("amount")]
public decimal Amount { get; set; }
```

---

## ? **2. TapService.cs - ����� �� �������**

### ������: ? **����� + �������**

**�����:** `AutoPartsStore.Infrastructure/Services/TapService.cs`

### **��������:**

#### �� �������:
```csharp
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", _tapSettings.SecretKey);
var response = await client.PostAsJsonAsync("https://api.tap.company/v2/charges", request);
```

#### �� �������: ? **�����**
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

### **��������� ��������:**
- ? ������ ������� ��������
- ? Logging ����
- ? Exception handling ����
- ? ��� Refund � Void

---

## ? **3. TapSettings - �����**

### ������: ? **�����**

**�����:** `AutoPartsStore.Core/Models/Payments/Tap/TapSettings.cs`

### �� �������:
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

### �� �������: ? **����� + ������**
```csharp
public class TapSettings
{
    public string SecretKey { get; set; } = null!;      // ?
    public string PublishableKey { get; set; } = null!; // ? �����
    public string BaseUrl { get; set; } = "https://api.tap.company/v2"; // ?
    public string WebhookUrl { get; set; } = null!;     // ? PostUrl
    public string RedirectUrl { get; set; } = null!;    // ?
    public bool Enable3DSecure { get; set; } = true;    // ? �����
    public bool SaveCards { get; set; } = false;        // ? �����
    public string? StatementDescriptor { get; set; }    // ? �����
}
```

---

## ? **4. PaymentService - ���� �� �������**

### ������: ? **����� + �����**

**�����:** `AutoPartsStore.Infrastructure/Services/PaymentService.cs`

### **��������:**

#### �� ������� (����):
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

#### �� ������� (�����): ? **����**
```csharp
var tapRequest = new TapCreateChargeRequest
{
    Amount = orderDto.TotalAmount,
    Currency = "SAR",
    ThreeDSecure = _tapSettings.Enable3DSecure,      // ? �����
    SaveCard = _tapSettings.SaveCards,               // ? �����
    Description = $"��� ��� {orderDto.OrderNumber}", // ? �����
    StatementDescriptor = _tapSettings.StatementDescriptor, // ? �����
    
    Metadata = new TapMetadata                       // ? ��� ������
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
    
    Customer = new TapCustomer                        // ? ������ �����
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
    
    Source = MapPaymentMethodToSource(request),       // ? ���
    Redirect = new TapRedirect { Url = request.RedirectUrl ?? _tapSettings.RedirectUrl },
    Post = new TapPost { Url = request.WebhookUrl ?? _tapSettings.WebhookUrl }
};
```

**������� ��������:**
- ? `Metadata` ���� ������
- ? `StatementDescriptor` ��������
- ? ������ ���� ���� �����
- ? ����� ��� ������ ��������
- ? ������ ������� �������

---

## ?? **5. Webhook Handler - �� ������**

### ������: ? **����� + ���**

### **�� ������ ������ - �������:**
```csharp
// ��� ������ �� hashstring (HMAC-SHA256)
var postedHashString = Request.Headers["hashstring"].FirstOrDefault();
using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_tap.SecretKey));
var computed = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(toBeHashed)))
    .ToLowerInvariant();

if (!computed.Equals(postedHashString, StringComparison.OrdinalIgnoreCase))
    return Unauthorized("Invalid signature");
```

### **�� ������� - ��� �������:**
```csharp
// ��� ���� ���� �� ��� signature ??
[HttpPost("webhook")]
[AllowAnonymous]
public async Task<IActionResult> TapWebhook([FromBody] TapWebhookPayload payload)
{
    var payment = await _paymentService.ProcessTapWebhookAsync(payload);
    return Ok(new { success = true });
}
```

### **���� - ��� �������:** ?
```csharp
// �� ����� TapWebhookValidator ?
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

**������� ������� �������:**
1. ? `TapWebhookValidator.cs` - ������ �� �������
2. ? ����� `PaymentsController.cs` - ������� ������
3. ? ����� ������ �� `Program.cs`

---

## ?? **���� �������� ������**

| ������ | �� ������� | �� ������� | ������ |
|--------|-------------|-------------|--------|
| **TapModels** | ? ����� | ? ���� + TapWebhookPayload | ? ����� |
| **TapService** | ? ����� | ? ����� + ������ ����� | ? ����� |
| **TapSettings** | ? ����� | ? ����� + Enable3DSecure | ? ����� |
| **PaymentService** | ? ���� | ? ����� + Metadata | ? ����� |
| **Webhook Validation** | ? ����� | ?? �� ��� ����� ? ? �� ������ | ? ����� |
| **Error Handling** | ? ��� ����� | ? ���� | ? ����� |
| **Logging** | ? ���� | ? ����� | ? ����� |
| **Source Mapping** | ? ���� (src_all) | ? ��� (��� ����� �����) | ? ����� |

---

## ?? **��������� �������**

### **1. TapWebhookValidator.cs** ? ����
```csharp
// ��� ����: AutoPartsStore.Infrastructure/Services/TapWebhookValidator.cs
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

### **2. ����� PaymentsController** ?
- ����� ������ �� `hashstring`
- ����� ��� raw body
- ������ �� ������� ��� ��������
- Logging �����

### **3. ����� �� Program.cs** ?
```csharp
builder.Services.AddScoped<TapWebhookValidator>();
```

---

## ?? **������**

### **���:**
```
Tap ? Webhook ? ? ���� ���� ����
```

### **����:**
```
Tap ? Webhook ? ? ���� �� hashstring ? ? ������ ����
```

**����� ������:**
```
1. ������� ������: id, amount, currency, gateway_ref, payment_ref, status, created
2. ����� ����: x_id{id}x_amount{amount}x_currency{currency}...
3. HMAC-SHA256 �������� Secret Key
4. ����� ��� Hex lowercase
5. ������ �� hashstring �������
```

---

## ? **�������**

### **������� �� AutoPartsStore:**

| ������� | ������� |
|---------|----------|
| **�������� �� Tap API** | ? 100% |
| **�������� �� �������** | ? 100% |
| **��������� ��������** | ? ����� |
| **������ (Webhook)** | ? ����� |
| **������ �������** | ? ����� |
| **Logging** | ? ���� |
| **�������** | ? ����� |
| **�������** | ? ����� |

### **������� �������: ?????**

**������� ����� ������ ������ ������� ������ + ������� ������!**

---

## ?? **������� �������**

### **1. ����� ������� Tap** ?
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

### **2. ����� Migration**
```bash
dotnet ef migrations add AddTapPaymentSupport
dotnet ef database update
```

### **3. ������ �� ������ Tap ���������**
```
Visa: 4111 1111 1111 1111
MasterCard: 5555 5555 5555 4444
CVV: 123
Expiry: 12/25
OTP: 123456
```

### **4. ����� Webhook �� Tap Dashboard**
```
1. ����� ������: https://dashboard.tap.company
2. Settings ? Webhooks
3. �����: https://yourstore.com/api/payments/webhook
4. �����: charge.created, charge.updated, charge.captured
```

---

## ?? **������� ��������**

? **������� �� Tap ����� 100% ������� ������**  
? **������� ���� �� ������� ������� (����� ��� �������)**  
? **������ ����� (Webhook validation)**  
? **����� ���� �������**  

**�� ���� ������ ����� .md ������ - �� ��� ���� �� �����!** ??
