using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities
{
    public class NutritionPlan : BaseEntity
    {
        public string PlanName { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<Meal> Meals { get; set; } = new List<Meal>();
        public ICollection<AppUser> Users { get; set; }
    }
}
