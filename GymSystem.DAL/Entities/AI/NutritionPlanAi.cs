using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities.AI
{
    public class NutritionPlanAi : BaseEntity
    {
        public string UserId { get; set; }
        public int Calories { get; set; }
        public bool IsDeleted { get; set; }
        public List<MealAi> Meals { get; set; }
    }
}
