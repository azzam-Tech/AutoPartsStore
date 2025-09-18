using AutoPartsStore.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPartsStore.Infrastructure.Configuration
{

    public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
    {
        public void Configure(EntityTypeBuilder<Favorite> builder)
        {
            builder.ToTable("Favorites");
            builder.HasKey(f => f.Id);

            builder.HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.CarPart)
                .WithMany()
                .HasForeignKey(f => f.PartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(f => f.AddedDate)
                .IsRequired();

            // منع تكرار المنتج في المفضلة لنفس المستخدم
            builder.HasIndex(f => new { f.UserId, f.PartId }).IsUnique();
        }
    }
}
