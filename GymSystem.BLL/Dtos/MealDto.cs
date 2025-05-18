using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos
{
    public class MealDto
    {
        public int MealId { get; set; }
        public string MealName { get; set; }
        public string Description { get; set; }
        public int? NutritionPlanId { get; set; }
        public int MealsCategoryId { get; set; }
    }
}
