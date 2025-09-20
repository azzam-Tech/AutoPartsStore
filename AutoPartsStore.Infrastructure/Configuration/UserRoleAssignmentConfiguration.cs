using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoPartsStore.Core.Entities;

namespace AutoPartsStore.Infrastructure.Configuration
{
    public class UserRoleAssignmentConfiguration : IEntityTypeConfiguration<UserRoleAssignment>
    {
        public void Configure(EntityTypeBuilder<UserRoleAssignment> builder)
        {
            builder.ToTable("UserRoleAssignments");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id);

            builder.HasOne(a => a.User)
                .WithMany(u => u.RoleAssignments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Role)
                .WithMany(r => r.Assignments)
                .HasForeignKey(a => a.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => new { a.UserId, a.RoleId }).IsUnique();
        }
    }
}