using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Infrastructure.Configuration
{
    public class ProductPromotionConfiguration : IEntityTypeConfiguration<ProductPromotion>
    {
        public void Configure(EntityTypeBuilder<ProductPromotion> builder)
        {
            builder.ToTable("ProductPromotions");
            builder.HasKey(pp => pp.Id);
            builder.Property(pp => pp.Id);

            builder.Property(p => p.CreatedAt)
                 .IsRequired();

            builder.HasOne(pp => pp.Promotion)
                .WithMany(p => p.ProductPromotions)
                .HasForeignKey(pp => pp.PromotionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pp => pp.CarPart)
                .WithMany(p => p.ProductPromotions)
                .HasForeignKey(pp => pp.PartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(pp => new { pp.PromotionId, pp.PartId }).IsUnique();
        }
    }









}