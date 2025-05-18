using GymSystem.BLL.Dtos.NutritionPlan;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GymSystem.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Security.Claims;
using GymSystem.DAL.Entities.Identity;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace GymSystem.API.Controllers
{
    public class AIController : BaseApiController
    {
        private readonly IGenerativeAIService _generativeAIService;
        private readonly UserManager<AppUser> _userManager;

        public AIController(
            IGenerativeAIService generativeAIService,
            UserManager<AppUser> userManager
        )
        {
            _generativeAIService = generativeAIService ?? throw new ArgumentNullException(nameof(generativeAIService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [Authorize]
        [HttpPost("generate-plan")]
        public async Task<IActionResult> GeneratePlanForUser()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiExceptionResponse(401, "User is not authenticated. Please log in."));
                }
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse(404, $"User with ID {userId} not found."));
                }

                var plan = await _generativeAIService.GeneratePlanAsync(user, userId);

                string message = "Plan generated successfully";
                if (plan.UsedDefaultValues)
                {
                    message += " Some fields were missing (CaloriesTarget, MealsPerDay, or TrainingDaysPerWeek), so default values were used.";
                }

                var workoutPlan = new Dictionary<string, object>();
                var days = plan.Exercises.Select(e => e.Day).Distinct().ToList();
                foreach (var day in days)
                {
                    if (string.IsNullOrEmpty(day))
                    {
                        continue;
                    }

                    if (day.ToLower().Contains("rest") || day.ToLower().Contains("recovery"))
                    {
                        var exercisesForDay = plan.Exercises
                            .Where(e => e.Day == day)
                            .Select(e => new
                            {
                                e.Name,
                                Sets = e.Sets > 0 ? e.Sets : (int?)null,
                                Reps = e.RepsText,
                                e.Category
                            })
                            .ToList();
                        workoutPlan[day] = exercisesForDay.Any() ? exercisesForDay : "Rest";
                    }
                    else
                    {
                        var exercisesForDay = plan.Exercises
                            .Where(e => e.Day == day)
                            .Select(e => new
                            {
                                e.Name,
                                e.Sets,
                                Reps = e.RepsText,
                                e.Category
                            })
                            .ToList();
                        workoutPlan[day] = exercisesForDay;
                    }
                }

                var nutritionPlan = new Dictionary<string, object>();
                var nutritionDays = plan.NutritionPlan.Meals.Select(m => m.Day).Distinct().ToList();
                if (!nutritionDays.Any())
                {
                    var defaultDays = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                    int caloriesTarget = user.CaloriesTarget ?? 2000;
                    int mealsPerDay = user.MealsPerDay ?? 3;
                    foreach (var day in defaultDays)
                    {
                        var defaultMeals = new List<object>();
                        int caloriesPerMeal = caloriesTarget / mealsPerDay;
                        for (int i = 1; i <= mealsPerDay; i++)
                        {
                            if (i == 1)
                                defaultMeals.Add(new { MealType = "Breakfast", Items = new[] { "Oatmeal (150g)", "Milk (100g)" }, Calories = caloriesPerMeal });
                            else if (i == 2)
                                defaultMeals.Add(new { MealType = "Lunch", Items = new[] { "Chicken Breast (150g)", "Brown Rice (100g)", "Vegetables (100g)" }, Calories = caloriesPerMeal });
                            else
                                defaultMeals.Add(new { MealType = $"Meal {i}", Items = new[] { "Grilled Salmon (120g)", "Quinoa (100g)", "Salad (100g)" }, Calories = caloriesPerMeal });
                        }
                        nutritionPlan[day] = defaultMeals;
                    }
                    message = "Plan generated, but no nutrition plan was provided by the AI. A default nutrition plan has been included.";
                }
                else
                {
                    foreach (var day in nutritionDays)
                    {
                        if (string.IsNullOrEmpty(day))
                        {
                            continue;
                        }

                        var mealsForDay = plan.NutritionPlan.Meals
                            .Where(m => m.Day == day)
                            .Select(m => new
                            {
                                MealType = m.MealType,
                                Items = m.Items,
                                Calories = m.Calories
                            })
                            .ToList();
                        nutritionPlan[day] = mealsForDay;
                    }
                }

                return Ok(new
                {
                    StatusCode = 200,
                    Message = message,
                    Data = new
                    {
                        Exercises = workoutPlan.Any() ? workoutPlan : null,
                        NutritionPlan = nutritionPlan.Any() ? nutritionPlan : null
                    }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse(500, $"Internal server error: {ex.Message}"));
            }
        }

        [Authorize]
        [HttpPut("update-fitness-data")]
        public async Task<IActionResult> UpdateUserFitnessData([FromBody] UpdateUserFitnessDataDto updateDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiExceptionResponse(401, "User is not authenticated. Please log in."));
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse(404, $"User with ID {userId} not found."));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResponse(400, "Validation failed: " + string.Join(", ", errors)));
                }

                if (updateDto.CaloriesTarget == null && updateDto.MealsPerDay == null && updateDto.TrainingDaysPerWeek == null)
                {
                    return BadRequest(new ApiResponse(400, "At least one field (CaloriesTarget, MealsPerDay, or TrainingDaysPerWeek) must be provided to update."));
                }


                user.CaloriesTarget = updateDto.CaloriesTarget;
                user.MealsPerDay = updateDto.MealsPerDay;
                user.TrainingDaysPerWeek = updateDto.TrainingDaysPerWeek;


                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = updateResult.Errors.Select(e => e.Description).ToList();
                    return BadRequest(new ApiResponse(400, "Failed to update user data: " + string.Join(", ", errors)));
                }

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "User fitness data updated successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse(500, $"Internal server error: {ex.Message}"));
            }
        }
    }
}