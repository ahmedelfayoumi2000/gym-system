using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos
{
    public class WorkoutPlanDto
    {
        public int? WorkoutPlanId { get; set; }
        public string? ImageUrl { get; set; }
        [NotMapped]
        public IFormFile Image { get; set; }
        public string PlanName { get; set; }
        public string Description { get; set; }
        public string? TrainerId { get; set; }
        public DayOfWeek DayOfWeek { get; set; } 
        public IEnumerable<ExerciseDto> Exercises { get; set; } 
    }
}
