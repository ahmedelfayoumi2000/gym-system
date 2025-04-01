using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos
{
    public class ExerciseDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Exercise name is required")]
        public string Name { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public int Repetitions { get; set; }
        public int Sets { get; set; }
        public uint DurationMinutes { get; set; }
        public int ExerciseCategoryId { get; set; }
        public string ExerciseCategoryName { get; set; }
        public int ExpectedCalories { get; set; }
        public int RestTimeSeconds { get; set; }


    }
}
