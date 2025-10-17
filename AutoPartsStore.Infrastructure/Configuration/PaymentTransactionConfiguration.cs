using AutoPartsStore.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPartsStore.Infrastructure.Configuration
{
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.ToTable("PaymentTransactions");
            builder.HasKey(pt => pt.Id);

            builder.Property(pt => pt.TapChargeId)  // Updated from MoyasarPaymentId
                .HasMaxLength(100);

            builder.HasIndex(pt => pt.TapChargeId);  // Updated from MoyasarPaymentId

            builder.Property(pt => pt.TransactionReference)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(pt => pt.TransactionReference)
                .IsUnique();

            builder.Property(pt => pt.PaymentMethod)
                .IsRequired();

            builder.Property(pt => pt.Status)
                .IsRequired();

            builder.Property(pt => pt.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(pt => pt.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("SAR");

            builder.Property(pt => pt.GatewayResponse)
                .HasColumnType("nvarchar(max)");

            builder.Property(pt => pt.AuthorizationCode)
                .HasMaxLength(100);

            builder.Property(pt => pt.ErrorMessage)
                .HasMaxLength(1000);

            builder.Property(pt => pt.ErrorCode)
                .HasMaxLength(50);

            builder.Property(pt => pt.CardLast4)
                .HasMaxLength(4);

            builder.Property(pt => pt.CardBrand)
                .HasMaxLength(50);

            builder.Property(pt => pt.CardScheme)  // New property
                .HasMaxLength(50);

            builder.Property(pt => pt.RefundedAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(pt => pt.RefundReason)
                .HasMaxLength(500);

            builder.Property(pt => pt.RefundReference)
                .HasMaxLength(100);

            // Relationships
            builder.HasOne(pt => pt.Order)
                .WithOne(o => o.PaymentTransaction)
                .HasForeignKey<PaymentTransaction>(pt => pt.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pt => pt.User)
                .WithMany()
                .HasForeignKey(pt => pt.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(pt => pt.OrderId);
            builder.HasIndex(pt => pt.UserId);
            builder.HasIndex(pt => pt.Status);
            builder.HasIndex(pt => pt.InitiatedDate);
            builder.HasIndex(pt => new { pt.UserId, pt.Status });
        }
    }
}
