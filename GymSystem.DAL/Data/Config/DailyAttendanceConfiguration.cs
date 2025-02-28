using GymSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.DAL.Configurations
{
    public class DailyAttendanceConfiguration : IEntityTypeConfiguration<DailyAttendance>
    {
        public void Configure(EntityTypeBuilder<DailyAttendance> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.UserId)
                   .IsRequired()
                   .HasMaxLength(450);

            builder.Property(a => a.ClassId)
                   .IsRequired();

            builder.Property(a => a.AttendanceDate)
                   .IsRequired();

            builder.Property(a => a.IsPresent)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.HasOne(a => a.User)
                   .WithMany(u => u.DailyAttendances)
                   .HasForeignKey(a => a.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Class)
                   .WithMany(c => c.DailyAttendances)
                   .HasForeignKey(a => a.ClassId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}