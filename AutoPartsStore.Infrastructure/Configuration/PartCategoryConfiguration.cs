using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Infrastructure.Configuration
{
    public class PartCategoryConfiguration : IEntityTypeConfiguration<PartCategory>
    {
        public void Configure(EntityTypeBuilder<PartCategory> builder)
        {
            builder.ToTable("PartCategories");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("CategoryID");

            builder.Property(c => c.CategoryName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.ImageUrl)
                .HasColumnName("ImageURL")
                .HasMaxLength(255);

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);
            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                   .HasDefaultValue(false);

            // Self-reference
            builder.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }









}