using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Infrastructure.Configuration
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.ToTable("Addresses");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id);

            builder.HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.District)
                .WithMany(d => d.Addresses)
                .HasForeignKey(a => a.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(a => a.StreetName)
                .HasMaxLength(150);

            builder.Property(a => a.StreetNumber)
                .HasMaxLength(20);

            builder.Property(a => a.PostalCode)
                .HasMaxLength(10);


            // Indexes
            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => a.DistrictId);
            builder.HasIndex(a => a.PostalCode);
        }
    }
}