using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities
{
    public class Exercise : BaseEntity
    {
        public string ExerciseName { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        [NotMapped]
        public IFormFile Image { get; set; }
        public int Repetitions { get; set; }
        public int Sets { get; set; }

        public int ExpectedCalories { get; set; }
        public int RestTimeSeconds { get; set; }
        public bool IsDeleted { get; set; }
        public int? WorkoutPlanId { get; set; }
        public WorkoutPlan? WorkoutPlan { get; set; }
        public int ExerciseCategoryId { get; set; }
        public ExerciseCategory ExerciseCategory { get; set; }
    }

}
