using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Infrastructure.Configuration
{
    public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
    {
        public void Configure(EntityTypeBuilder<Promotion> builder)
        {
            builder.ToTable("Promotions");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("PromotionID");

            builder.Property(p => p.PromotionName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Description)
                .HasMaxLength(500);

            builder.Property(p => p.DiscountType)
                .IsRequired()
                .HasMaxLength(1)
                .HasConversion(
                    v => v.ToString(),
                    v => v[0]);

            builder.Property(p => p.DiscountValue)
                .IsRequired()
                .HasColumnType("DECIMAL(10,2)");

            builder.Property(p => p.StartDate)
                .IsRequired();

            builder.Property(p => p.EndDate)
                .IsRequired();

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.MinOrderAmount)
                .HasDefaultValue(0)
                .HasColumnType("DECIMAL(10,2)");
        }
    }









}