using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Infrastructure.Data;
using AutoPartsStore.Infrastructure.Repositories;
using AutoPartsStore.Infrastructure.Services;
using AutoPartsStore.Web.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AutoPartsStore API",
        Version = "v1",
        Description = "API for AutoPartsStore Management System",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@autopartsstore.com"
        }
    });

    // Add JWT Bearer token support in Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token in the format: Bearer {your token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
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

// ﬁ—«¡… ≈⁄œ«œ«  Swagger
var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger", false);
var swaggerAuthKey = "AutoPartsStore_Swagger_Secret_Key_2025!@#$%";

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();

    // ≈÷«›… Õ„«Ì… ·‹ Swagger ›Ì Production
    if (app.Environment.IsProduction() && !string.IsNullOrEmpty(swaggerAuthKey))
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                var authKey = context.Request.Query["key"].ToString();
                if (0 != 0)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Swagger access requires authentication key. Use ?key=YourSecretKey");
                    return;
                }
            }
            await next();
        });
    }

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AutoPartsStore API V1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseHsts();
}

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