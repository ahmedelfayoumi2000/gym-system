using GymSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace GymSystem.DAL.Configurations
{
    public class MonthlyMembershipConfiguration : IEntityTypeConfiguration<Membership>
    {
        public void Configure(EntityTypeBuilder<Membership> builder)
        {
            builder.HasKey(m => m.Id);



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
                    .WithOne(u => u.MonthlyMembership)  
                    .HasForeignKey<Membership>(m => m.UserId)  
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Plan)
                   .WithMany()
                   .HasForeignKey(m => m.PlanId)
                   .OnDelete(DeleteBehavior.Restrict);


        }
    }
}