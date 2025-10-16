using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Interfaces.IRepositories;
using AutoPartsStore.Core.Interfaces.IServices;
using AutoPartsStore.Core.Interfaces.IServices.IEmailSirvices;
using AutoPartsStore.Core.Models;
using AutoPartsStore.Core.Models.Payments.Moyasar;
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
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(options =>
{
    // Add model validation filter globally
    options.Filters.Add<ModelValidationFilter>();
});

// Add error handling services
builder.Services.AddErrorHandling();

// Add Swagger services
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "AutoParts Store API", 
        Version = "v1",
        Description = "API for managing auto parts store operations"
    });

    // Add JWT Auth to Swagger
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

// Database with advanced configuration
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

    // Enable detailed errors only in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors(
            configuration.GetValue<bool>("DatabaseSettings:EnableDetailedErrors", true));
        options.EnableSensitiveDataLogging(
            configuration.GetValue<bool>("DatabaseSettings:EnableSensitiveDataLogging", true));
    }
});

// Repositories
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

// Services
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
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddRateLimiting(builder.Configuration);
builder.Services.AddScoped<JwtTokenGenerator>();

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
    
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// Get JWT configuration
var jwtKey = builder.Configuration["Jwt:Key"]
             ?? throw new InvalidOperationException("JWT Key is not configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AutoPartsStore.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AutoPartsStore.Client";

// Authentication
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

// Error handling middleware (MUST BE FIRST after dev exception page)
app.UseErrorHandling();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Rate limiting
app.UseRateLimiting();

// Admin check middleware
app.UseAdminCheck();

// Status code middleware (for additional status code handling)
app.UseStatusCodeHandling();

app.MapControllers();

app.Run();