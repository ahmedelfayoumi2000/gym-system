using GymSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.DAL.Configurations
{
    public class MonthlyMembershipConfiguration : IEntityTypeConfiguration<MonthlyMembership>
    {
        public void Configure(EntityTypeBuilder<MonthlyMembership> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.UserId)
                   .IsRequired()
                   .HasMaxLength(450);

            builder.Property(m => m.ClassId)
                   .IsRequired();

            builder.Property(m => m.PlanId)
                   .IsRequired();

            builder.Property(m => m.StartDate)
                   .IsRequired();

            builder.Property(m => m.EndDate)
                   .IsRequired();

            builder.Property(m => m.IsActive)
                   .IsRequired()
                   .HasDefaultValue(true);

            builder.HasOne(m => m.User)
                   .WithMany(u => u.MonthlyMemberships)
                   .HasForeignKey(m => m.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Class)
                   .WithMany(c => c.MonthlyMemberships)
                   .HasForeignKey(m => m.ClassId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Plan)
                   .WithMany()
                   .HasForeignKey(m => m.PlanId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}