using GymSystem.DAL.Entities.Enums.Business;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.User
{
    public class UpdateLevelDto
    {
        [Required(ErrorMessage = "Fitness level is required")]
        public FitnessLevel FitnessLevel { get; set; }

    }

}
