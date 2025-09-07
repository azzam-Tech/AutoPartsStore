using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Infrastructure.Configuration
{
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.ToTable("Suppliers");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id);

            builder.Property(s => s.SupplierName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.ContactPerson)
                .HasMaxLength(100);

            builder.Property(s => s.Email)
                .HasMaxLength(100)
                .IsUnicode(false);

            builder.Property(s => s.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);

            builder.Property(s => s.Address)
                .HasMaxLength(255);

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                   .HasDefaultValue(false);
        }
    }









}