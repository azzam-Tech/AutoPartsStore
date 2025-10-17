using AutoPartsStore.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPartsStore.Infrastructure.Configuration
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");
            builder.HasKey(oi => oi.Id);

            builder.Property(oi => oi.PartNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(oi => oi.PartName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(oi => oi.ImageUrl)
                .HasMaxLength(500);

            builder.Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(oi => oi.DiscountPercent)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.Property(oi => oi.Quantity)
                .IsRequired();

            builder.Property(oi => oi.PromotionName)
                .HasMaxLength(200);

            builder.Property(oi => oi.PromotionDiscountValue)
                .HasColumnType("decimal(18,2)");

            builder.Property(oi => oi.SubTotal)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(oi => oi.DiscountAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(oi => oi.FinalPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(oi => oi.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Relationships
            builder.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(oi => oi.CarPart)
                .WithMany()
                .HasForeignKey(oi => oi.PartId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(oi => oi.OrderId);
            builder.HasIndex(oi => oi.PartId);
        }
    }
}
