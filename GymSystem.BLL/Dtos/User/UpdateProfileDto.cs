using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.User
{
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Age is required")]
        [Range(1, 150, ErrorMessage = "Age must be between 1 and 150")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Weight is required")]
        [Range(1, 500, ErrorMessage = "Weight must be between 1 and 500 kg")]
        public float Weight { get; set; }

        [Required(ErrorMessage = "Height is required")]
        [Range(1, 300, ErrorMessage = "Height must be between 1 and 300 cm")]
        public float Height { get; set; }
    }
}
