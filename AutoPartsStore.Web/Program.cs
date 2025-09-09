using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Infrastructure.Data;
using AutoPartsStore.Infrastructure.Repositories;
using AutoPartsStore.Infrastructure.Services;
using AutoPartsStore.Web.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add Swagger services
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // ≈⁄œ«œ «·‹ JWT Auth
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "«ﬂ » ﬂ·„… 'Bearer' »⁄œÂ« ›—«€ À„ «· Êﬂ‰. „À«·: Bearer 12345abcdef"
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

    // «· ›⁄Ì· ›ﬁÿ ›Ì Ê÷⁄ «· ÿÊÌ—
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors(
            configuration.GetValue<bool>("DatabaseSettings:EnableDetailedErrors", true));
        options.EnableSensitiveDataLogging(
            configuration.GetValue<bool>("DatabaseSettings:EnableSensitiveDataLogging", true));
    }
});

//// Â–« «·ﬂÊœ „Œ’’ ·⁄„·Ì«  EF Core Tools („À· Migration)
//if (args.Length > 0 && args[0].Contains("ef", StringComparison.OrdinalIgnoreCase))
//{
//    //  ﬂÊÌ‰ »”Ìÿ ·‹ DbContext ·⁄„·Ì«  «· ’„Ì„
//    builder.Services.AddDbContext<AppDbContext>(options =>
//        options.UseSqlServer("Server=LAPTOP-2AR5OF7M;Database=AutoPartsStoreDb;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true;"));
//}


// Repositories
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPartCategoryRepository, PartCategoryRepository>();
builder.Services.AddScoped<IPartCategoryService, PartCategoryService>();
builder.Services.AddScoped<ICarPartRepository, CarPartRepository>();
builder.Services.AddScoped<ICarPartService, CarPartService>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IDistrictRepository, DistrictRepository>();
builder.Services.AddScoped<IDistrictService, DistrictService>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
builder.Services.AddScoped<IProductPromotionRepository, ProductPromotionRepository>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();

// ﬁ—«¡… ≈⁄œ«œ«  JWT „‰ Configuration
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
});

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();



app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
// ›Ì Program.cs° ﬁ»· app.MapControllers();
app.UseAdminCheck();
app.MapControllers();

// Error Handling
app.UseExceptionHandler("/error");

app.Run();