using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities
{
    public class Recipe : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Ingredients { get; set; }
        public string PreparationSteps { get; set; }
        public int PreparationTimeMinutes { get; set; }
        public int Calories { get; set; }
        public int MealsCategoryId { get; set; }
        public MealsCategory MealsCategory { get; set; }
        public bool IsDeleted { get; set; }
    }
}
