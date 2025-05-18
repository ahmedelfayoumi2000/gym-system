using GymSystem.DAL.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace GymSystem.DAL.Configurations
{
    public class ClassConfiguration : IEntityTypeConfiguration<Class>
    {
        public void Configure(EntityTypeBuilder<Class> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.MemberName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(c => c.StartTime)
                   .IsRequired();

            builder.Property(c => c.IsDeleted)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.HasOne(c => c.Trainer)
                   .WithMany(t => t.Classes)
                   .HasForeignKey(c => c.TrainerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(c => !c.IsDeleted);

        }
    }
}