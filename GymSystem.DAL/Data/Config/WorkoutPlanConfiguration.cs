using GymSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Data.Config
{
    public class WorkoutPlanConfiguration : IEntityTypeConfiguration<WorkoutPlan>
    {
        public void Configure(EntityTypeBuilder<WorkoutPlan> builder)
        {
            builder.HasKey(wp => wp.Id);
            builder.HasOne(wp => wp.Trainer)
                   .WithMany(u => u.WorkoutPlans)
                   .HasForeignKey(wp => wp.TrainerId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasQueryFilter(u => !u.IsDeleted);
        }
    }
}
