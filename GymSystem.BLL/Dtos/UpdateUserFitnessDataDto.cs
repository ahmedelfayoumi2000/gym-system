using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos
{
    public class UpdateUserFitnessDataDto
    {
        [Required(ErrorMessage = "CaloriesTarget is required.")]
        [Range(1200, 5000, ErrorMessage = "CaloriesTarget must be between 1200 and 5000.")]
        public int CaloriesTarget { get; set; }

        [Required(ErrorMessage = "MealsPerDay is required.")]
        [Range(1, 8, ErrorMessage = "MealsPerDay must be between 1 and 8.")]
        public int MealsPerDay { get; set; }

        [Required(ErrorMessage = "TrainingDaysPerWeek is required.")]
        [Range(1, 7, ErrorMessage = "TrainingDaysPerWeek must be between 1 and 7.")]
        public int TrainingDaysPerWeek { get; set; }
    }
}
