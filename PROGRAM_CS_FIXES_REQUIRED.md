# ?? CONFIGURATION ISSUES & FIXES

## ?? Critical Issues Found in Program.cs

### 1. **Duplicate Repository Registrations** ?
**Lines 121-123:** Repositories registered twice
```csharp
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();  // Line 121
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();          // Line 122
builder.Services.AddScoped<IProductReviewRepository, ProductReviewRepository>(); // Line 123
```
These are duplicated on lines 124-126!

### 2. **Duplicate Payment Service Registration** ?
**Lines 151 & 156:**
```csharp
builder.Services.AddScoped<IPaymentService, PaymentService>();  // Line 151
// ... other code ...
builder.Services.AddScoped<IPaymentService, PaymentService>();  // Line 156 - DUPLICATE!
```

### 3. **Old Moyasar Code Still Present** ?
**Lines 158-174:** Moyasar configuration should be removed
```csharp
// ? REMOVE THIS - No longer needed
builder.Services.Configure<MoyasarSettings>(
    builder.Configuration.GetSection("Moyasar"));

builder.Services.AddHttpClient<IMoyasarService, MoyasarService>(...);
```

### 4. **Missing Using Statements for Moyasar** ?
**Lines referring to IMoyasarService and MoyasarSettings** will cause compilation errors since:
- `IMoyasarService` interface doesn't exist (or shouldn't be used)
- `MoyasarSettings` model references the old gateway
- Missing `using AutoPartsStore.Core.Models.Payments.Moyasar;`
- Missing `using Microsoft.Extensions.Options;`
- Missing `using System.Net.Http.Headers;`

---

## ? CORRECTED Program.cs

Here's the clean version you should use:

```csharp
using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Interfaces.IRepositories;
using AutoPartsStore.Core.Interfaces.IServices;
using AutoPartsStore.Core.Interfaces.IServices.IEmailSirvices;
using AutoPartsStore.Core.Models;
using AutoPartsStore.Core.Models.Payments.Tap;
using AutoPartsStore.Infrastructure.Data;
using AutoPartsStore.Infrastructure.Repositories;
using AutoPartsStore.Infrastructure.Services;
using AutoPartsStore.Infrastructure.Services.EmailServices;
using AutoPartsStore.Infrastructure.Utils;
using AutoPartsStore.Web.Extensions;
using AutoPartsStore.Web.Filters;
using AutoPartsStore.Web.Middleware;
using AutoPartsStore.Web.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ModelValidationFilter>();
});

// Error handling
builder.Services.AddErrorHandling();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "AutoParts Store API", 
        Version = "v1",
        Description = "API for managing auto parts store operations with Tap Payment Gateway"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "√œŒ· ﬂ·„… 'Bearer' „ »Ê⁄… »„”«›… À„ «· Êﬂ‰. „À«·: Bearer 12345abcdef"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Database
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: configuration.GetValue<int>("DatabaseSettings:MaxRetryCount", 3),
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);

        sqlOptions.CommandTimeout(configuration.GetValue<int?>("DatabaseSettings:CommandTimeout") ?? 30);
    });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors(
            configuration.GetValue<bool>("DatabaseSettings:EnableDetailedErrors", true));
        options.EnableSensitiveDataLogging(
            configuration.GetValue<bool>("DatabaseSettings:EnableSensitiveDataLogging", true));
    }
});

// ? Repositories (NO DUPLICATES)
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<IPartCategoryRepository, PartCategoryRepository>();
builder.Services.AddScoped<ICarPartRepository, CarPartRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IDistrictRepository, DistrictRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
builder.Services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<ICustomerFeedbackRepository, CustomerFeedbackRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();

// ? Services (NO DUPLICATES)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPartCategoryService, PartCategoryService>();
builder.Services.AddScoped<ICarPartService, CarPartService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IDistrictService, DistrictService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IProductReviewService, ProductReviewService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICustomerFeedbackService, CustomerFeedbackService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<JwtTokenGenerator>();

// ? TAP PAYMENT GATEWAY (Clean configuration)
builder.Services.Configure<TapSettings>(
    builder.Configuration.GetSection("TapSettings"));

builder.Services.AddHttpClient<ITapService, TapService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

builder.Services.AddScoped<IPaymentService, PaymentService>();

// Rate limiting
builder.Services.AddRateLimiting(builder.Configuration);

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
             ?? throw new InvalidOperationException("JWT Key is not configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AutoPartsStore.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AutoPartsStore.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
.AddJwtBearer("JwtBearer", jwtOptions =>
{
    jwtOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RequireExpirationTime = true
    };

    jwtOptions.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();

            var response = new ApiResponse
            {
                Success = false,
                Message = "€Ì— „’—Õ. Ì—ÃÏ  ”ÃÌ· «·œŒÊ·.",
                Errors = new List<string> { "Ì ÿ·»  Ê›Ì—  Êﬂ‰ ’«·Õ ›Ì —√” «·ÿ·» (Authorization: Bearer <token>)" }
            };

            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsJsonAsync(response);
        },

        OnAuthenticationFailed = async context =>
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = "›‘· «· Õﬁﬁ „‰ «· Êﬂ‰.",
                Errors = new List<string> { "«· Êﬂ‰ €Ì— ’«·Õ √Ê «‰ Â  ’·«ÕÌ Â." }
            };

            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsJsonAsync(response);
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AutoParts Store API V1");
        c.DocExpansion(DocExpansion.None);
    });
}

// Middleware pipeline
app.UseErrorHandling();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiting();
app.UseAdminCheck();
app.UseStatusCodeHandling();

app.MapControllers();

app.Run();
```

---

## ? CORRECTED appsettings.json

Remove old Moyasar section:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=SQL5110.site4now.net;Initial Catalog=db_abdf41_autopartstoredb;User Id=db_abdf41_autopartstoredb_admin;Password=3e%P@8BtY_j7PO889;"
  },
  "DatabaseSettings": {
    "MaxRetryCount": 3,
    "CommandTimeout": 30,
    "EnableDetailedErrors": true,
    "EnableSensitiveDataLogging": true
  },
  "Jwt": {
    "Key": "AutoPartsStore_Jwt_Secret_2025!@#$%^&*()_+{}:<>?|~`1234567890",
    "Issuer": "AutoPartsStore.Api",
    "Audience": "AutoPartsStore.Client"
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "molhemautopartstore@gmail.com",
    "Password": "fqybggamjqklzhny",
    "EnableSsl": true
  },
  "TapSettings": {
    "SecretKey": "sk_test_XKokBfNWv6FIYuTMg5sLPjhJ",
    "PublishableKey": "pk_test_EtHFV4BuPQokJT6jiROls87Y",
    "BaseUrl": "https://api.tap.company/v2",
    "WebhookUrl": "https://yourstore.com/api/payments/webhook",
    "RedirectUrl": "https://yourstore.com/payment-result",
    "Enable3DSecure": true,
    "SaveCards": false,
    "StatementDescriptor": "AutoPartsStore"
  },
  "EmergencySettings": {
    "EMERGENCY_ADMIN_KEY": "AutoPartsStore_Emergency_2025!@#$%^&*()_+{}:<>?"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

---

## ?? Summary of Changes

### ? REMOVED:
1. Duplicate repository registrations (3 duplicates)
2. Duplicate `IPaymentService` registration
3. All Moyasar configuration and services
4. `Moyasar` section from appsettings.json

### ? KEPT/ADDED:
1. Clean Tap configuration
2. Single registration of all services
3. Proper HttpClient for Tap with lifetime management
4. All necessary middleware

---

## ? Verification Checklist

### Configuration
- [x] TapSettings configured in appsettings.json
- [x] TapSettings registered in Program.cs
- [x] ITapService and TapService registered
- [x] IPaymentService registered (once!)
- [x] HttpClient configured for Tap
- [ ] Moyasar references removed

### Controllers
- [x] PaymentsController updated for Tap
- [x] Webhook endpoint configured
- [x] All payment endpoints ready

### Services
- [x] TapService implemented
- [x] PaymentService updated for Tap
- [x] Payment repository updated

### Database
- [ ] Migration created and applied
- [ ] MoyasarPaymentId renamed to TapChargeId
- [ ] CardScheme column added

---

## ?? Next Steps

### 1. Fix Program.cs
Copy the corrected `Program.cs` above to replace your current one.

### 2. Update appsettings.json
Remove the `Moyasar` section completely.

### 3. Run Database Migration
```bash
dotnet ef migrations add RenameMoyasarToTap
dotnet ef database update
```

### 4. Build and Test
```bash
dotnet build
dotnet run
```

### 5. Test Payment APIs
- Test `/api/payments/initiate`
- Test `/api/payments/webhook`
- Test `/api/payments/verify/{chargeId}`

---

## ?? Important Notes

1. **Never register a service twice** - It causes unpredictable behavior
2. **Remove all Moyasar references** - They're no longer needed
3. **Test thoroughly** - Especially webhook functionality
4. **Use test keys** - Don't use live keys until fully tested

---

## ?? Need Help?

If you encounter errors:
1. Check the error message carefully
2. Verify all using statements are present
3. Ensure database migration was applied
4. Check Tap Dashboard for webhook logs

**Status:** Ready to fix and test! ??
