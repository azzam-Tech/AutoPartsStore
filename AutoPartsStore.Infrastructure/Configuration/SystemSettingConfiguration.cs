using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Infrastructure.Configuration
{
    public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
    {
        public void Configure(EntityTypeBuilder<SystemSetting> builder)
        {
            builder.ToTable("SystemSettings");
            builder.HasKey(ss => ss.Id);
            builder.Property(ss => ss.Id).HasColumnName("SettingID");

            builder.Property(ss => ss.SettingKey)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);

            builder.Property(ss => ss.SettingValue)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(ss => ss.Description)
                .HasMaxLength(255);

            builder.Property(ss => ss.Category)
                .HasMaxLength(50);

            builder.HasIndex(ss => ss.SettingKey).IsUnique();
        }
    }









}