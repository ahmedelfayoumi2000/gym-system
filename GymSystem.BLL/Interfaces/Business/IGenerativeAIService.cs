using GymSystem.DAL.Entities.Enums.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IGenerativeAIService
    {
        Task<AIGeneratedPlan> GeneratePlanAsync(AIInputData inputData);
    }

    public class AIInputData
    {
        public int UserId { get; set; }
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
    }

    public class AIExercise
    {
        public string Name { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public string Category { get; set; }
    }

    public class AINutritionPlan
    {
        public int Calories { get; set; }
        public List<AIMeal> Meals { get; set; }
    }

    public class AIMeal
    {
        public string Name { get; set; }
        public List<string> Items { get; set; }
        public int Calories { get; set; }
    }
}
