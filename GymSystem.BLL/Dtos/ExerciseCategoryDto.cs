using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos
{
    public class ExerciseCategoryDto
    {
        public int? Id { get; set; }
        public string CategoryName { get; set; }
        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
