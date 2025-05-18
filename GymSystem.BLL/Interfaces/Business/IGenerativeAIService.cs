using GymSystem.DAL.Entities.Enums.Business;
using GymSystem.DAL.Entities.Identity;
using System.Collections.Generic;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IGenerativeAIService
    {
        Task<AIGeneratedPlan> GeneratePlanAsync(AppUser user, string userId);
    }

    public class AIInputData
    {
        public string UserId { get; set; }
        public float? Weight { get; set; }
        public float? Height { get; set; }
        public uint? Age { get; set; }
        public string Gender { get; set; }
        public FitnessLevel? FitnessLevel { get; set; }
        public Goal? Goal { get; set; }
        public int CaloriesTarget { get; set; }
        public int MealsPerDay { get; set; }
        public int TrainingDaysPerWeek { get; set; }
    }

    public class AIGeneratedPlan
    {
        public List<AIExercise> Exercises { get; set; }
        public AINutritionPlan NutritionPlan { get; set; }
        public bool UsedDefaultValues { get; set; }
        public string WarmUp { get; set; }
        public string CoolDown { get; set; }
        public List<string> AdditionalNotes { get; set; }
    }

    public class AIExercise
    {
        public string Name { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; } // For backward compatibility, but we'll use RepsText
        public string RepsText { get; set; } // To store the original Reps string like "10-12" or "45 seconds"
        public string Category { get; set; }
        public string Day { get; set; }
    }

    public class AINutritionPlan
    {
        public int Calories { get; set; }
        public List<AIMeal> Meals { get; set; }
    }

    public class AIMeal
    {
        public string Name { get; set; }
        public string MealType { get; set; } 
        public List<string> Items { get; set; }
        public int Calories { get; set; }
        public string Day { get; set; }
    }
}