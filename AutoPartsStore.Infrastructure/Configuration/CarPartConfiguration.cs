using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Infrastructure.Configuration
{
    public class CarPartConfiguration : IEntityTypeConfiguration<CarPart>
    {
        public void Configure(EntityTypeBuilder<CarPart> builder)
        {
            builder.ToTable("CarParts");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id);

            builder.Property(p => p.PartNumber)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(p => p.PartName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.CarBrand)
                .HasMaxLength(100);

            builder.Property(p => p.CarModel)
                .HasMaxLength(100);

            builder.Property(p => p.CarYear)
                .HasMaxLength(100);

            builder.Property(p => p.UnitPrice)
                .IsRequired()
                .HasColumnType("DECIMAL(10,2)");

            builder.Property(p => p.DiscountPercent)
                .HasDefaultValue(0)
                .HasColumnType("DECIMAL(5,2)");

            builder.Property(p => p.StockQuantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.UpdatedAt);

            builder.Property(p => p.ImageUrl)
                .HasColumnName("ImageURL")
                .HasMaxLength(255);
            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                   .HasDefaultValue(false);

            // Relationships
            builder.HasOne(p => p.Category)
                .WithMany(c => c.CarParts)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(p => p.PartNumber).IsUnique();
            builder.HasIndex(p => p.PartName);
            builder.HasIndex(p => p.CategoryId);
            builder.HasIndex(p => p.CarBrand);
            builder.HasIndex(p => p.CarModel);
            builder.HasIndex(p => p.IsActive);
            builder.HasIndex(p => p.IsDeleted);
            builder.HasIndex(p => p.UnitPrice);
            builder.HasIndex(p => new { p.IsActive, p.IsDeleted });
        }
    }









}