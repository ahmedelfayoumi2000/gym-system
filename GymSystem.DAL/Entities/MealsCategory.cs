using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities
{
    public class MealsCategory : BaseEntity
    {
        public string? ImageUrl { get; set; }
        [NotMapped]
        public IFormFile Image { get; set; }
        public string CategoryName { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<Meal> Meals { get; set; }
        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    }
}
