// DAL/Data/Config/AttendanceConfiguration.cs
using GymSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.DAL.Data.Config
{
    public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {
            builder.HasOne(a => a.Class)
                   .WithMany(c => c.Attendances) 
                   .HasForeignKey(a => a.ClassId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired(false);

            //the attending user
            builder.HasOne(a => a.User)
                   .WithMany() 
                   .HasForeignKey(a => a.UserId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired();

            // the staff who recorded the attendance
            builder.HasOne(a => a.CreatedByUser)
                   .WithMany() 
                   .HasForeignKey(a => a.CreatedByUserId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired();

     
        }
    }
}