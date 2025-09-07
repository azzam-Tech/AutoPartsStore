using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Infrastructure.Configuration
{
    public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
    {
        public void Configure(EntityTypeBuilder<ProductReview> builder)
        {
            builder.ToTable("ProductReviews");
            builder.HasKey(pr => pr.Id);
            builder.Property(pr => pr.Id);

            builder.HasOne(pr => pr.CarPart)
                .WithMany(p => p.Reviews)
                .HasForeignKey(pr => pr.PartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pr => pr.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(pr => pr.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(pr => pr.Rating)
                .IsRequired();

            builder.Property(pr => pr.ReviewText)
                .HasMaxLength(1000);

            builder.Property(pr => pr.ReviewDate)
                .IsRequired();

            builder.Property(pr => pr.IsApproved)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(pr => new { pr.PartId, pr.UserId }).IsUnique();
        }
    }









}