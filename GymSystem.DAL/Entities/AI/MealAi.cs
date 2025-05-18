using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities.AI
{
    public class MealAi : BaseEntity
    {
        public string Name { get; set; }
        public string Items { get; set; }
        public int Calories { get; set; }
        public int NutritionPlanId { get; set; }
        public NutritionPlanAi NutritionPlan { get; set; }
    }
}
