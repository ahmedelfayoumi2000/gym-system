using GymSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.DAL.Data.Config
{
    public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {

            builder.HasOne(m => m.Membership)
                .WithMany(u => u.Attendances)
                .HasForeignKey(a => a.MembershipId)
                .OnDelete(DeleteBehavior.Cascade);
          
        }
    }
}