using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Infrastructure.Configuration
{
    public class CustomerFeedbackConfiguration : IEntityTypeConfiguration<CustomerFeedback>
    {
        public void Configure(EntityTypeBuilder<CustomerFeedback> builder)
        {
            builder.ToTable("CustomerFeedbacks");
            builder.HasKey(cf => cf.Id);
            builder.Property(cf => cf.Id).HasColumnName("FeedbackID");

            builder.Property(cf => cf.UserId)
                .IsRequired();

            builder.Property(cf => cf.FeedbackType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(cf => cf.Message)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(cf => cf.Rate)
                .IsRequired()
                .HasDefaultValue(5); // قيمة افتراضية 5 نجوم

            builder.Property(cf => cf.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(cf => cf.IsFeatured)
                .IsRequired(false)
                .HasDefaultValue(null);

            // Relationships
            builder.HasOne(cf => cf.User)
                .WithMany()
                .HasForeignKey(cf => cf.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(cf => cf.UserId);
            builder.HasIndex(cf => cf.FeedbackType);
            builder.HasIndex(cf => cf.Rate);
            builder.HasIndex(cf => cf.CreatedDate);
        }
    }
}