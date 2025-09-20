using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Infrastructure.Configuration
{
    public class InventoryLogConfiguration : IEntityTypeConfiguration<InventoryLog>
    {
        public void Configure(EntityTypeBuilder<InventoryLog> builder)
        {
            builder.ToTable("InventoryLogs");
            builder.HasKey(il => il.Id);
            builder.Property(il => il.Id);

            builder.HasOne(il => il.CarPart)
                .WithMany(p => p.InventoryLogs)
                .HasForeignKey(il => il.PartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(il => il.ChangedByUser)
                .WithMany()
                .HasForeignKey(il => il.ChangedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(il => il.ChangeType)
                .IsRequired()
                .HasMaxLength(1)
                .HasConversion(
                    v => v.ToString(),
                    v => v[0]);

            builder.Property(il => il.Quantity)
                .IsRequired();

            builder.Property(il => il.PreviousQuantity)
                .IsRequired();

            builder.Property(il => il.NewQuantity)
                .IsRequired();

            builder.Property(il => il.ChangeDate)
                .IsRequired();

            builder.Property(il => il.Notes)
                .HasMaxLength(500);
        }
    }
}