using AutoPartsStore.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AutoPartsStore.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
        }



        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserRoleAssignment> UserRoleAssignments { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<PartCategory> PartCategories { get; set; }
        public DbSet<CarPart> CarParts { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PartSupply> PartSupplies { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<ProductPromotion> ProductPromotions { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<InventoryLog> InventoryLogs { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // للتطوير فقط - لا تستخدم في Production
                optionsBuilder.UseSqlServer("Server=db26732.public.databaseasp.net; Database=db26732; User Id=db26732; Password=3e%P@8BtY_j7; Encrypt=False; MultipleActiveResultSets=True;");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // تطبيق جميع التكوينات من نفس التجميع
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}