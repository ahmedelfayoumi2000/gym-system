using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GymSystem.BLL.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Services.Business;
using System.ComponentModel.DataAnnotations;

namespace GymSystem.BLL.Services
{
    public class GeminiService : IGenerativeAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly IDistributedCache _cache;
        private readonly PlanValidator _validator;
        public GeminiService(HttpClient httpClient, string apiKey, IDistributedCache cache ,PlanValidator validator)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent";
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<AIGeneratedPlan> GeneratePlanAsync(AIInputData inputData)
        {
            // بنعمل كيي  لي الكاش عشان كل يوزر يكون عندو كاش مختلف
            var cacheKey = $"GeminiPlan_{inputData.UserId}_{inputData.Weight}_{inputData.Height}_{inputData.Age}_{inputData.Gender}_{inputData.FitnessLevel}_{inputData.Goal}";

            var cachedPlan = await _cache.GetStringAsync(cacheKey);
            // Gemini لو الخطة مش موجودة في الكاش بنرجعها ولو مش موجودة بنولدها من 
            if (!string.IsNullOrEmpty(cachedPlan))
            {
                return JsonConvert.DeserializeObject<AIGeneratedPlan>(cachedPlan);
            }

            var prompt = BuildPrompt(inputData);
            var requestBody = BuildRequestBody(prompt);
            var responseContent = await SendRequestToGemini(requestBody);
            var plan = ParseResponse(responseContent);

            _validator.ValidatePlan(plan); 
            //بنخزنها في الكاش ساعة 
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };
            await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(plan), cacheOptions);

            return plan;
        }

        private string BuildPrompt(AIInputData inputData)
        {
            return $"Generate a workout plan and nutrition plan for a {inputData.Age}-year-old {inputData.Gender}, " +
                   $"{inputData.Weight}kg, {inputData.Height}cm, {inputData.FitnessLevel} fitness level, " +
                   $"goal is {inputData.Goal}, targeting {inputData.CaloriesTarget} calories, " +
                   $"{inputData.MealsPerDay} meals per day, training {inputData.TrainingDaysPerWeek} days a week.";
        }

        private StringContent BuildRequestBody(string prompt)
        {
            var requestData = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var jsonContent = JsonConvert.SerializeObject(requestData);
            return new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }

        private async Task<string> SendRequestToGemini(StringContent content)
        {
            try
            {
                var requestUri = $"{_apiUrl}?key={_apiKey}";
                var response = await _httpClient.PostAsync(requestUri, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to communicate with Gemini API: {ex.Message}", ex);
            }
        }

        private AIGeneratedPlan ParseResponse(string responseContent)
        {
            var jsonResponse = JObject.Parse(responseContent);
            var generatedText = jsonResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

            if (string.IsNullOrEmpty(generatedText))
            {
                throw new Exception("Failed to parse response from Gemini API.");
            }

            return ParseGeneratedText(generatedText);
        }

        private AIGeneratedPlan ParseGeneratedText(string generatedText)
        {
            var lines = generatedText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var plan = new AIGeneratedPlan
            {
                Exercises = new List<AIExercise>(),
                NutritionPlan = new AINutritionPlan { Meals = new List<AIMeal>() }
            };

            bool isExerciseSection = false;
            bool isNutritionSection = false;

            foreach (var line in lines)
            {
                if (line.StartsWith("WorkoutPlan:"))
                {
                    isExerciseSection = true;
                    isNutritionSection = false;
                    continue;
                }
                else if (line.StartsWith("NutritionPlan:"))
                {
                    isExerciseSection = false;
                    isNutritionSection = true;
                    continue;
                }

                if (isExerciseSection && line.StartsWith("-"))
                {
                    var parts = line.Split(':');
                    var nameSetsReps = parts[0].Replace("- ", "").Trim();
                    var setsReps = parts[1].Split(',');

                    var exercise = new AIExercise
                    {
                        Name = nameSetsReps,
                        Sets = int.Parse(setsReps[0].Replace("sets", "").Trim()),
                        Reps = int.Parse(setsReps[1].Replace("reps", "").Trim()),
                        Category = "Strength"
                    };
                    plan.Exercises.Add(exercise);
                }
                else if (isNutritionSection && line.StartsWith("-"))
                {
                    var parts = line.Split('(');
                    var nameItems = parts[0].Replace("- ", "").Trim();
                    var itemsCalories = parts[1].Replace(")", "").Trim();

                    var meal = new AIMeal
                    {
                        Name = nameItems.Split(':')[0].Trim(),
                        Items = nameItems.Split(':')[1].Split(',').Select(item => item.Trim()).ToList(),
                        Calories = int.Parse(itemsCalories.Replace("calories", "").Trim())
                    };
                    plan.NutritionPlan.Meals.Add(meal);
                }
                else if (line.StartsWith("Total Calories:"))
                {
                    plan.NutritionPlan.Calories = int.Parse(line.Replace("Total Calories:", "").Trim());
                }
            }

            return plan;
        }
    }
}