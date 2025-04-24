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
    public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
    {
        public void Configure(EntityTypeBuilder<Exercise> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.ExerciseCategory)
                   .WithMany(ec => ec.Exercises)
                   .HasForeignKey(e => e.ExerciseCategoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(u => !u.IsDeleted);
        }
    }
}
