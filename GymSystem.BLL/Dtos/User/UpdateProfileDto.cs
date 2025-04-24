using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GymSystem.DAL.Entities.Enums.Business;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.User
{
    public class UpdateProfileDto
    {
        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
        public string? FullName { get; set; }
        public Gender? Gender { get; set; }
        public string? Email { get; set; }

        [Range(1, 150, ErrorMessage = "Age must be between 1 and 150")]
        public uint? Age { get; set; }

        [Range(1, 500, ErrorMessage = "Weight must be between 1 and 500 kg")]
        public float? Weight { get; set; }

        [Range(1, 300, ErrorMessage = "Height must be between 1 and 300 cm")]
        public float? Height { get; set; }
    }
}
