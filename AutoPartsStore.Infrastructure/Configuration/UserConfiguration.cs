using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Infrastructure.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id);

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);

            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);

            builder.Property(u => u.RegistrationDate)
                .IsRequired();

            builder.Property(u => u.LastLoginDate);

            builder.Property(u => u.LastLocationUpdate);

            builder.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.IsDeleted)
                   .HasDefaultValue(false);

            builder.Property(p => p.CreatedAt)
                   .IsRequired();

            builder.Property(p => p.UpdatedAt);
            // Indexes
            builder.HasIndex(u => u.Username).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();
        }
    }
}