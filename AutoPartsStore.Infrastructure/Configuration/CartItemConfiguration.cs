using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Infrastructure.Configuration
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItems");
            builder.HasKey(ci => ci.Id);
            builder.Property(ci => ci.Id);

            builder.HasOne(ci => ci.Cart)
                .WithMany(sc => sc.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ci => ci.CarPart)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.PartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(ci => ci.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(ci => ci.CreatedAt)
                .IsRequired();

            builder.HasIndex(ci => new { ci.CartId, ci.PartId }).IsUnique();
        }
    }









}