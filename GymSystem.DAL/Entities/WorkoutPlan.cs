using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities
{
    public class WorkoutPlan : BaseEntity
    {
        public string? ImageUrl { get; set; }
        [NotMapped]
        public IFormFile Image { get; set; }
        public string PlanName { get; set; }
        public string Description { get; set; }
        public DayOfWeek DayOfWeek { get; set; } 
        public bool IsDeleted { get; set; }
        public string? TrainerId { get; set; }
        public AppUser? Trainer { get; set; }
        public ICollection<Exercise> Exercises { get; set; }
    }
}
