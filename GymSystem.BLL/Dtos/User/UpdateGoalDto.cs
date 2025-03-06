using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.User
{
    public class UpdateGoalDto
    {
        [Required(ErrorMessage = "Goal is required")]
        public string Goal { get; set; } // "WeightLoss", "WeightGain", "MuscleGain", "BetterBodyShape"
    }
}
