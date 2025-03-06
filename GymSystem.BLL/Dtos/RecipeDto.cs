using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos
{
    public class RecipeDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Recipe name is required")]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Ingredients { get; set; }
        public string PreparationSteps { get; set; }
        public int PreparationTimeMinutes { get; set; }
        public int Calories { get; set; }
        public int MealsCategoryId { get; set; }
        public string MealsCategoryName { get; set; }
    }
}
