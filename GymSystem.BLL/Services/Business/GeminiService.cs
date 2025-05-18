using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Services.Business;
using GymSystem.DAL.Entities;
using AutoMapper;
using GymSystem.DAL.Entities.Identity;

namespace GymSystem.BLL.Services
{
    public class GeminiService : IGenerativeAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly PlanValidator _validator;
        private readonly IMapper _mapper;

        public GeminiService(HttpClient httpClient, string apiKey, PlanValidator validator, IMapper mapper)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent";
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<AIGeneratedPlan> GeneratePlanAsync(AppUser user, string userId)
        {
            if (user.Weight == null || user.Height == null || user.Age == null ||
                string.IsNullOrEmpty(user.Gender) || user.FitnessLevel == null || user.Goal == null)
            {
                throw new ArgumentException("User data is incomplete. Please provide weight, height, age, gender, fitness level, and goal.");
            }

            int caloriesTarget = user.CaloriesTarget ?? 2000;
            int mealsPerDay = user.MealsPerDay ?? 3;
            int trainingDaysPerWeek = user.TrainingDaysPerWeek ?? 4;

            var inputData = _mapper.Map<AIInputData>(user);
            inputData.UserId = userId;
            inputData.CaloriesTarget = caloriesTarget;
            inputData.MealsPerDay = mealsPerDay;
            inputData.TrainingDaysPerWeek = trainingDaysPerWeek;

            var prompt = BuildPrompt(inputData);
            var requestBody = BuildRequestBody(prompt);
            var responseContent = await SendRequestToGemini(requestBody);
            var plan = ParseResponse(responseContent);

            _validator.ValidatePlan(plan);

            plan.UsedDefaultValues = !user.CaloriesTarget.HasValue || !user.MealsPerDay.HasValue || !user.TrainingDaysPerWeek.HasValue;

            return plan;
        }

        private string BuildPrompt(AIInputData inputData)
        {
            return $@"
                     Act like a certified personal trainer and registered nutritionist with over 15 years of experience designing fully personalized fitness and diet programs for people of all backgrounds.
                     
                     Your task is to create a highly detailed and structured weekly workout and nutrition plan in JSON format. This plan must be tailored based on the following input data:
                     - Age: {inputData.Age}
                     - Gender: {inputData.Gender}
                     - Weight: {inputData.Weight}kg
                     - Height: {inputData.Height}cm
                     - Fitness Level: {inputData.FitnessLevel}
                     - Goal: {inputData.Goal}
                     - Target Daily Calories: {inputData.CaloriesTarget}
                     - Meals Per Day: {inputData.MealsPerDay}
                     - Training Days Per Week: {inputData.TrainingDaysPerWeek}
                     
                     ### Requirements for the JSON Output:
                     1. The output must be a valid JSON object with a single key 'Data' containing two keys: 'Exercises' and 'NutritionPlan'.
                     2. Do NOT include any comments, placeholders, or extra text outside the JSON structure.
                     3. All values (Sets, Reps, Calories, weights) must be properly quoted strings.
                     4. The structure must be consistent across all days and meals.

                     ### Exercises Structure:
                     - 'Exercises' must be a dictionary with exactly {inputData.TrainingDaysPerWeek} keys, each representing a training day.
                     - The keys must be named as 'Day 1: [Focus]', 'Day 2: [Focus]', etc., up to the number of training days.
                     - The focus for each day must be one of the following: 'Upper Body', 'Lower Body & Core', 'Full Body', 'Cardio & Core'.
                     - For the remaining days (up to 7), add keys like 'Day 3: Rest', 'Day 4: Rest', etc., with the value being an empty array [].
                     - Each training day must contain exactly 5 exercises.
                     - Each exercise must have exactly these fields:
                       - 'Name': A string (e.g., 'Barbell Bench Press').
                       - 'Sets': A string (e.g., '3').
                       - 'Reps': A string (e.g., '8-12' or '30-60 seconds hold').
                       - 'Category': A string (e.g., 'Strength', 'Core', 'Cardio', 'HIIT').
                     - Ensure exercises match the day's focus (e.g., Upper Body days should have exercises like Bench Press, Rows, etc.).

                     ### NutritionPlan Structure:
                     - 'NutritionPlan' must be a dictionary with exactly 7 keys: 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'.
                     - Each day must contain exactly {inputData.MealsPerDay} meals.
                     - Each meal must have exactly these fields:
                       - 'MealType': A string (e.g., 'Breakfast', 'Snack 1', 'Lunch', 'Snack 2', 'Dinner').
                       - 'Ingredients': An array of objects, each with 'item' (e.g., 'Chicken Breast') and 'weight' in grams as a string (e.g., '150').
                       - 'Calories': A string (e.g., '400').
                     - Meals should include a variety of protein sources (chicken, salmon, turkey, eggs, etc.), complex carbs (brown rice, quinoa, sweet potato), and vegetables.
                     - Ensure the total calories per day are close to {inputData.CaloriesTarget} (distribute evenly across meals).

                     ### Example Output Format:
                     ```json
                     {{
                       ""Data"": {{
                         ""Exercises"": {{
                           ""Day 1: Upper Body"": [
                             {{ ""Name"": ""Barbell Bench Press"", ""Sets"": ""3"", ""Reps"": ""8-12"", ""Category"": ""Strength"" }},
                             {{ ""Name"": ""Dumbbell Rows"", ""Sets"": ""3"", ""Reps"": ""8-12"", ""Category"": ""Strength"" }},
                             {{ ""Name"": ""Overhead Press"", ""Sets"": ""3"", ""Reps"": ""8-12"", ""Category"": ""Strength"" }},
                             {{ ""Name"": ""Bicep Curls"", ""Sets"": ""3"", ""Reps"": ""10-15"", ""Category"": ""Strength"" }},
                             {{ ""Name"": ""Triceps Pushdowns"", ""Sets"": ""3"", ""Reps"": ""10-15"", ""Category"": ""Strength"" }}
                           ],
                           ""Day 2: Lower Body & Core"": [
                             {{ ""Name"": ""Barbell Squats"", ""Sets"": ""3"", ""Reps"": ""8-12"", ""Category"": ""Strength"" }},
                             {{ ""Name"": ""Romanian Deadlifts"", ""Sets"": ""3"", ""Reps"": ""10-15"", ""Category"": ""Strength"" }},
                             {{ ""Name"": ""Leg Press"", ""Sets"": ""3"", ""Reps"": ""12-15"", ""Category"": ""Strength"" }},
                             {{ ""Name"": ""Plank"", ""Sets"": ""3"", ""Reps"": ""30-60 seconds hold"", ""Category"": ""Core"" }},
                             {{ ""Name"": ""Crunches"", ""Sets"": ""3"", ""Reps"": ""15-20"", ""Category"": ""Core"" }}
                           ],
                           ""Day 3: Rest"": [],
                           ""Day 4: Rest"": [],
                           ""Day 5: Rest"": [],
                           ""Day 6: Rest"": [],
                           ""Day 7: Rest"": []
                         }},
                         ""NutritionPlan"": {{
                           ""Monday"": [
                             {{ ""MealType"": ""Breakfast"", ""Ingredients"": [{{ ""item"": ""Oatmeal"", ""weight"": ""50"" }}, {{ ""item"": ""Banana"", ""weight"": ""100"" }}], ""Calories"": ""350"" }},
                             {{ ""MealType"": ""Snack 1"", ""Ingredients"": [{{ ""item"": ""Greek Yogurt"", ""weight"": ""150"" }}], ""Calories"": ""200"" }},
                             {{ ""MealType"": ""Lunch"", ""Ingredients"": [{{ ""item"": ""Chicken Breast"", ""weight"": ""150"" }}, {{ ""item"": ""Brown Rice"", ""weight"": ""100"" }}], ""Calories"": ""400"" }},
                             {{ ""MealType"": ""Snack 2"", ""Ingredients"": [{{ ""item"": ""Almonds"", ""weight"": ""30"" }}], ""Calories"": ""200"" }},
                             {{ ""MealType"": ""Dinner"", ""Ingredients"": [{{ ""item"": ""Salmon"", ""weight"": ""150"" }}, {{ ""item"": ""Sweet Potato"", ""weight"": ""150"" }}], ""Calories"": ""450"" }}
                           ],
                           ""Tuesday"": [
                             {{ ""MealType"": ""Breakfast"", ""Ingredients"": [{{ ""item"": ""Eggs"", ""weight"": ""100"" }}, {{ ""item"": ""Whole Wheat Toast"", ""weight"": ""50"" }}], ""Calories"": ""300"" }},
                             {{ ""MealType"": ""Snack 1"", ""Ingredients"": [{{ ""item"": ""Protein Shake"", ""weight"": ""250"" }}], ""Calories"": ""250"" }},
                             {{ ""MealType"": ""Lunch"", ""Ingredients"": [{{ ""item"": ""Turkey Breast"", ""weight"": ""150"" }}, {{ ""item"": ""Quinoa"", ""weight"": ""100"" }}], ""Calories"": ""350"" }},
                             {{ ""MealType"": ""Snack 2"", ""Ingredients"": [{{ ""item"": ""Cottage Cheese"", ""weight"": ""150"" }}], ""Calories"": ""150"" }},
                             {{ ""MealType"": ""Dinner"", ""Ingredients"": [{{ ""item"": ""Lean Ground Beef"", ""weight"": ""150"" }}, {{ ""item"": ""Brown Rice"", ""weight"": ""100"" }}], ""Calories"": ""400"" }}
                           ],
                           ""Wednesday"": [
                             {{ ""MealType"": ""Breakfast"", ""Ingredients"": [{{ ""item"": ""Oatmeal"", ""weight"": ""50"" }}, {{ ""item"": ""Berries"", ""weight"": ""100"" }}], ""Calories"": ""300"" }},
                             {{ ""MealType"": ""Snack 1"", ""Ingredients"": [{{ ""item"": ""Greek Yogurt"", ""weight"": ""150"" }}], ""Calories"": ""200"" }},
                             {{ ""MealType"": ""Lunch"", ""Ingredients"": [{{ ""item"": ""Chicken Breast"", ""weight"": ""150"" }}, {{ ""item"": ""Quinoa"", ""weight"": ""100"" }}], ""Calories"": ""400"" }},
                             {{ ""MealType"": ""Snack 2"", ""Ingredients"": [{{ ""item"": ""Almonds"", ""weight"": ""30"" }}], ""Calories"": ""200"" }},
                             {{ ""MealType"": ""Dinner"", ""Ingredients"": [{{ ""item"": ""Salmon"", ""weight"": ""150"" }}, {{ ""item"": ""Sweet Potato"", ""weight"": ""150"" }}], ""Calories"": ""450"" }}
                           ],
                           ""Thursday"": [
                             {{ ""MealType"": ""Breakfast"", ""Ingredients"": [{{ ""item"": ""Eggs"", ""weight"": ""100"" }}, {{ ""item"": ""Whole Wheat Toast"", ""weight"": ""50"" }}], ""Calories"": ""300"" }},
                             {{ ""MealType"": ""Snack 1"", ""Ingredients"": [{{ ""item"": ""Protein Shake"", ""weight"": ""250"" }}], ""Calories"": ""250"" }},
                             {{ ""MealType"": ""Lunch"", ""Ingredients"": [{{ ""item"": ""Turkey Breast"", ""weight"": ""150"" }}, {{ ""item"": ""Brown Rice"", ""weight"": ""100"" }}], ""Calories"": ""350"" }},
                             {{ ""MealType"": ""Snack 2"", ""Ingredients"": [{{ ""item"": ""Cottage Cheese"", ""weight"": ""150"" }}], ""Calories"": ""150"" }},
                             {{ ""MealType"": ""Dinner"", ""Ingredients"": [{{ ""item"": ""Lean Ground Beef"", ""weight"": ""150"" }}, {{ ""item"": ""Brown Rice"", ""weight"": ""100"" }}], ""Calories"": ""400"" }}
                           ],
                           ""Friday"": [
                             {{ ""MealType"": ""Breakfast"", ""Ingredients"": [{{ ""item"": ""Oatmeal"", ""weight"": ""50"" }}, {{ ""item"": ""Banana"", ""weight"": ""100"" }}], ""Calories"": ""350"" }},
                             {{ ""MealType"": ""Snack 1"", ""Ingredients"": [{{ ""item"": ""Greek Yogurt"", ""weight"": ""150"" }}], ""Calories"": ""200"" }},
                             {{ ""MealType"": ""Lunch"", ""Ingredients"": [{{ ""item"": ""Chicken Breast"", ""weight"": ""150"" }}, {{ ""item"": ""Quinoa"", ""weight"": ""100"" }}], ""Calories"": ""400"" }},
                             {{ ""MealType"": ""Snack 2"", ""Ingredients"": [{{ ""item"": ""Almonds"", ""weight"": ""30"" }}], ""Calories"": ""200"" }},
                             {{ ""MealType"": ""Dinner"", ""Ingredients"": [{{ ""item"": ""Salmon"", ""weight"": ""150"" }}, {{ ""item"": ""Sweet Potato"", ""weight"": ""150"" }}], ""Calories"": ""450"" }}
                           ],
                           ""Saturday"": [
                             {{ ""MealType"": ""Breakfast"", ""Ingredients"": [{{ ""item"": ""Eggs"", ""weight"": ""100"" }}, {{ ""item"": ""Whole Wheat Toast"", ""weight"": ""50"" }}], ""Calories"": ""300"" }},
                             {{ ""MealType"": ""Snack 1"", ""Ingredients"": [{{ ""item"": ""Protein Shake"", ""weight"": ""250"" }}], ""Calories"": ""250"" }},
                             {{ ""MealType"": ""Lunch"", ""Ingredients"": [{{ ""item"": ""Turkey Breast"", ""weight"": ""150"" }}, {{ ""item"": ""Brown Rice"", ""weight"": ""100"" }}], ""Calories"": ""350"" }},
                             {{ ""MealType"": ""Snack 2"", ""Ingredients"": [{{ ""item"": ""Cottage Cheese"", ""weight"": ""150"" }}], ""Calories"": ""150"" }},
                             {{ ""MealType"": ""Dinner"", ""Ingredients"": [{{ ""item"": ""Lean Ground Beef"", ""weight"": ""150"" }}, {{ ""item"": ""Brown Rice"", ""weight"": ""100"" }}], ""Calories"": ""400"" }}
                           ],
                           ""Sunday"": [
                             {{ ""MealType"": ""Breakfast"", ""Ingredients"": [{{ ""item"": ""Oatmeal"", ""weight"": ""50"" }}, {{ ""item"": ""Berries"", ""weight"": ""100"" }}], ""Calories"": ""300"" }},
                             {{ ""MealType"": ""Snack 1"", ""Ingredients"": [{{ ""item"": ""Greek Yogurt"", ""weight"": ""150"" }}], ""Calories"": ""200"" }},
                             {{ ""MealType"": ""Lunch"", ""Ingredients"": [{{ ""item"": ""Chicken Breast"", ""weight"": ""150"" }}, {{ ""item"": ""Quinoa"", ""weight"": ""100"" }}], ""Calories"": ""400"" }},
                             {{ ""MealType"": ""Snack 2"", ""Ingredients"": [{{ ""item"": ""Almonds"", ""weight"": ""30"" }}], ""Calories"": ""200"" }},
                             {{ ""MealType"": ""Dinner"", ""Ingredients"": [{{ ""item"": ""Salmon"", ""weight"": ""150"" }}, {{ ""item"": ""Sweet Potato"", ""weight"": ""150"" }}], ""Calories"": ""450"" }}
                           ]
                         }}
                       }}
                     }}";
        }

        private StringContent BuildRequestBody(string prompt)
        {
            var requestData = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestData);
            return new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }

        private async Task<string> SendRequestToGemini(StringContent content)
        {
            try
            {
                var requestUri = $"{_apiUrl}?key={_apiKey}";
                var response = await _httpClient.PostAsync(requestUri, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    responseString = responseString.Trim();
                    int jsonStart = responseString.IndexOf('{');
                    int jsonEnd = responseString.LastIndexOf('}');
                    if (jsonStart >= 0 && jsonEnd > jsonStart)
                    {
                        responseString = responseString.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    }
                    else
                    {
                        throw new Exception("Invalid JSON structure in response.");
                    }

                    using var doc = JsonDocument.Parse(responseString);
                    var text = doc.RootElement
                                  .GetProperty("candidates")[0]
                                  .GetProperty("content")
                                  .GetProperty("parts")[0]
                                  .GetProperty("text")
                                  .GetString();

                    if (string.IsNullOrEmpty(text))
                    {
                        throw new Exception("No text content received from Gemini API.");
                    }

                    text = text.Trim();
                    jsonStart = text.IndexOf('{');
                    jsonEnd = text.LastIndexOf('}');
                    if (jsonStart >= 0 && jsonEnd > jsonStart)
                    {
                        text = text.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    }
                    else
                    {
                        throw new Exception("No valid JSON object found in response.");
                    }

                    return text;
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to communicate with Gemini API. Status: {response.StatusCode}, Response: {errorBody}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to communicate with Gemini API: {ex.Message}", ex);
            }
        }

        private AIGeneratedPlan ParseResponse(string generatedText)
        {
            try
            {
                generatedText = RemoveComments(generatedText);
                generatedText = generatedText.Trim();
                int jsonStart = generatedText.IndexOf('{');
                if (jsonStart >= 0)
                {
                    generatedText = generatedText.Substring(jsonStart);
                }

                var jsonResponse = JsonSerializer.Deserialize<JsonApiResponse>(generatedText);
                if (jsonResponse == null || jsonResponse.Data == null)
                {
                    throw new Exception("Invalid JSON response from Gemini API: Missing Data.");
                }

                var plan = new AIGeneratedPlan
                {
                    Exercises = new List<AIExercise>(),
                    NutritionPlan = new AINutritionPlan { Meals = new List<AIMeal>() },
                    WarmUp = "",
                    CoolDown = "",
                    AdditionalNotes = new List<string>
                    {
                        "Calorie Target: The 1200 calorie target is very low for a 50-year-old male, even at 70kg and 120cm. It needs significant adjustment (potentially doubling) for muscle gain.",
                        "Height of 120cm is highly unusual for an adult male. This data point needs verification as it affects BMI and nutritional needs.",
                        "Meal Repetition: Wednesday-Sunday meal plans are omitted for brevity but should include varied meals with similar calorie targets and macronutrient ratios.",
                        "Macronutrient Ratio: The plan lacks specific macronutrient ratios. For muscle gain, a higher protein intake (1.6-2.2g/kg body weight) is crucial.",
                        "Individual Needs: This is a template. A certified professional should adjust based on dietary restrictions, preferences, and medical history."
                    }
                };

                if (jsonResponse.Data?.Exercises != null)
                {
                    foreach (var dayEntry in jsonResponse.Data.Exercises)
                    {
                        string day = dayEntry.Key;
                        if (dayEntry.Value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var exerciseElement in element.EnumerateArray())
                            {
                                var repsText = exerciseElement.GetProperty("Reps").GetString().Trim();
                                var setsText = exerciseElement.GetProperty("Sets").GetString().Trim();
                                if (!int.TryParse(setsText, out int sets))
                                {
                                    throw new Exception($"Invalid Sets value '{setsText}' for exercise '{exerciseElement.GetProperty("Name").GetString()}' on {day}.");
                                }

                                var exercise = new AIExercise
                                {
                                    Name = exerciseElement.GetProperty("Name").GetString(),
                                    Sets = sets,
                                    RepsText = repsText.Contains("seconds") ? repsText : repsText.Replace("-", " to "), // Normalize RepsText
                                    Category = exerciseElement.GetProperty("Category").GetString(),
                                    Day = day
                                };
                                if (repsText.Contains("-") && int.TryParse(repsText.Split("-")[0], out int minReps))
                                {
                                    exercise.Reps = minReps; 
                                }
                                else if (repsText.Contains("seconds"))
                                {
                                    exercise.Reps = -1; 
                                }
                                else if (int.TryParse(repsText, out int singleReps))
                                {
                                    exercise.Reps = singleReps;
                                }
                                else
                                {
                                    exercise.Reps = -1;
                                }
                                plan.Exercises.Add(exercise);
                            }
                        }
                    }
                }

                if (jsonResponse.Data?.NutritionPlan != null)
                {
                    foreach (var dayEntry in jsonResponse.Data.NutritionPlan)
                    {
                        string day = dayEntry.Key;
                        if (dayEntry.Value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var mealElement in element.EnumerateArray())
                            {
                                var items = new List<string>();
                                foreach (var ingredient in mealElement.GetProperty("Ingredients").EnumerateArray())
                                {
                                    var item = ingredient.GetProperty("item").GetString();
                                    var weight = ingredient.GetProperty("weight").GetString();
                                    items.Add($"{item} – {weight}g");
                                }

                                var caloriesText = mealElement.GetProperty("Calories").GetString();
                                if (!int.TryParse(caloriesText, out int calories))
                                {
                                    throw new Exception($"Invalid Calories value '{caloriesText}' for meal '{mealElement.GetProperty("MealType").GetString()}' on {day}.");
                                }

                                var meal = new AIMeal
                                {
                                    Name = mealElement.GetProperty("MealType").GetString(),
                                    Calories = calories,
                                    Items = items,
                                    Day = day,
                                    MealType = mealElement.GetProperty("MealType").GetString()
                                };
                                plan.NutritionPlan.Meals.Add(meal);
                            }
                        }
                    }
                }

                return plan;
            }
            catch (JsonException ex)
            {
                throw new Exception($"Failed to parse JSON response: {ex.Message}");
            }
        }

        private string RemoveComments(string text)
        {
            while (text.Contains("/*"))
            {
                int start = text.IndexOf("/*");
                int end = text.IndexOf("*/", start);
                if (end == -1) break;
                text = text.Substring(0, start) + text.Substring(end + 2);
            }

            var lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                int commentStart = lines[i].IndexOf("//");
                if (commentStart >= 0)
                {
                    lines[i] = lines[i].Substring(0, commentStart);
                }
            }

            return string.Join("\n", lines).Trim();
        }
    }

    internal class JsonApiResponse
    {
        public JsonData Data { get; set; }
    }

    internal class JsonData
    {
        public Dictionary<string, JsonElement> Exercises { get; set; }
        public Dictionary<string, JsonElement> NutritionPlan { get; set; }
    }
}