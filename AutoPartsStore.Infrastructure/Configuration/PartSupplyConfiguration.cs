using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Infrastructure.Configuration
{
    public class PartSupplyConfiguration : IEntityTypeConfiguration<PartSupply>
    {
        public void Configure(EntityTypeBuilder<PartSupply> builder)
        {
            builder.ToTable("PartSupplies");
            builder.HasKey(ps => ps.Id);
            builder.Property(ps => ps.Id);

            builder.HasOne(ps => ps.CarPart)
                .WithMany(p => p.Supplies)
                .HasForeignKey(ps => ps.PartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ps => ps.Supplier)
                .WithMany(s => s.Supplies)
                .HasForeignKey(ps => ps.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(ps => ps.SupplyPrice)
                .IsRequired()
                .HasColumnType("DECIMAL(10,2)");

            builder.Property(ps => ps.LastSupplyDate);

            builder.HasIndex(ps => new { ps.PartId, ps.SupplierId }).IsUnique();
        }
    }









}