using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.ExerciseSpec;
using GymSystem.BLL.Specifications.MembershipSpec;
using GymSystem.BLL.Specifications.UserFavoriteExerciseSpec;
using GymSystem.BLL.Specifications.WorkoutPlanSpec;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories.Business
{
    public class ExerciseRepo : IExerciseRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ExerciseRepo> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IGenerativeAIService _aiService;

        public ExerciseRepo(IUnitOfWork unitOfWork,
                             IMapper mapper,
                             ILogger<ExerciseRepo> logger,
                             UserManager<AppUser> userManager,
                             IImageService imageService,
                             IGenerativeAIService aiService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _imageService = imageService;
            _aiService = aiService;
        }


        public async Task<ApiResponse> GenerateExercisesForUserAsync(int userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return new ApiResponse(404, $"User with ID {userId} not found.");
                }

                var aiInput = new AIInputData
                {
                    UserId = userId,
                    Weight = user.Weight,
                    Height = user.Height,
                    Age = user.Age,
                    Gender = user.Gender,
                    FitnessLevel = user.FitnessLevel,
                    Goal = user.Goal,
                    //لو هنضفهم في ال AppUser
                    //CaloriesTarget = user.CaloriesTarget,
                    //MealsPerDay = user.MealsPerDay,
                    //TrainingDaysPerWeek = user.TrainingDaysPerWeek
                };
                AIGeneratedPlan aiPlan;
                try
                {
                    aiPlan = await _aiService.GeneratePlanAsync(aiInput);
                }
                catch (Exception ex)
                {
                    return new ApiResponse(503, $"Failed to generate exercises using AI: {ex.Message}. Please add exercises manually as a trainer.");
                }

                var exercises = aiPlan.Exercises.Select(aiExercise => new Exercise
                {
                    Sets = aiExercise.Sets,
                    //لو هنضفهم برضو
                    //Name = aiExercise.Name,
                    //Reps = aiExercise.Reps,
                    //Category = aiExercise.Category,
                    //UserId = userId,
                    IsDeleted = false
                }).ToList();

                foreach (var exercise in exercises)
                {
                    _unitOfWork.Repository<Exercise>().Add(exercise);
                }

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to save exercises to the database.");
                }

                return new ApiResponse(201, "Exercises generated and saved successfully", _mapper.Map<List<ExerciseDto>>(exercises));
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, $"Error generating exercises: {ex.Message}");
            }
        }
    

        public async Task<ExerciseDto> AddExerciseAsync(ExerciseDto exerciseDto)
        {
            try
            {

                var category = await _unitOfWork.Repository<ExerciseCategory>().GetByIdAsync(exerciseDto.ExerciseCategoryId);
                if (category == null)
                {
                    throw new ApplicationException("Invalid ExerciseCategory ID");
                }

                var exerciseEntity = _mapper.Map<Exercise>(exerciseDto);
                exerciseEntity.ExerciseCategory = category;

                if (exerciseDto.Image != null)
                {
                    var uploadResult = await _imageService.UploadImageAsync(exerciseDto.Image);
                    if (uploadResult.Item1 == 1)
                    {
                        exerciseEntity.ImageUrl = uploadResult.Item2;
                    }
                    else
                    {
                        throw new ApplicationException($"Failed to Upload Image: {uploadResult.Item2}");
                    }
                }

                await _unitOfWork.Repository<Exercise>().Add(exerciseEntity);
                var result = await _unitOfWork.Complete();

                if (result <= 0)
                {
                    _logger.LogError("Failed to save exercise to database.");
                    throw new ApplicationException("Failed to save exercise");
                }

                var createdDto = _mapper.Map<ExerciseDto>(exerciseEntity);
                _logger.LogInformation("Exercise added successfully with ID: {Id}", exerciseEntity.Id);
                return createdDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding exercise.");
                throw new ApplicationException($"Failed to add exercise: {ex.Message}", ex);
            }
        }

        public async Task<ExerciseDto> UpdateExerciseAsync(int id, ExerciseDto exerciseDto)
        {
            try
            {
                _logger.LogInformation("Updating exercise with ID: {Id}", id);

                var existingExercise = await _unitOfWork.Repository<Exercise>().GetByIdAsync(id);
                if (existingExercise == null || existingExercise.IsDeleted)
                {
                    _logger.LogWarning("Exercise with ID {Id} not found or deleted.", id);
                    throw new ApplicationException("Exercise not found");
                }

                var category = await _unitOfWork.Repository<ExerciseCategory>().GetByIdAsync(exerciseDto.ExerciseCategoryId);
                if (category == null)
                {
                    _logger.LogWarning("ExerciseCategory with ID {Id} not found.", exerciseDto.ExerciseCategoryId);
                    throw new ApplicationException("Invalid ExerciseCategory ID");
                }

                _mapper.Map(exerciseDto, existingExercise);
                existingExercise.ExerciseCategory = category;

                if (exerciseDto.Image != null)
                {

                    if (!string.IsNullOrEmpty(existingExercise.ImageUrl))
                    {
                        await _imageService.DeleteImageAsync(existingExercise.ImageUrl);
                    }
                    var uploadResult = await _imageService.UploadImageAsync(exerciseDto.Image);
                    if (uploadResult.Item1 == 1)
                    {
                        existingExercise.ImageUrl = uploadResult.Item2;
                    }
                    else
                    {
                        throw new ApplicationException($"Failed to Upload Image: {uploadResult.Item2}");
                    }
                }

                _unitOfWork.Repository<Exercise>().Update(existingExercise);
                var result = await _unitOfWork.Complete();

                if (result <= 0)
                {
                    _logger.LogError("Failed to update exercise with ID: {Id}", id);
                    throw new ApplicationException("Failed to update exercise");
                }

                var updatedDto = _mapper.Map<ExerciseDto>(existingExercise);
                _logger.LogInformation("Exercise updated successfully with ID: {Id}", id);
                return updatedDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exercise with ID: {Id}", id);
                throw new ApplicationException($"Failed to update exercise: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse> DeleteExercise(int id)
        {
            try
            {
                var existingExercise = await _unitOfWork.Repository<Exercise>().GetByIdAsync(id);
                if (existingExercise == null || existingExercise.IsDeleted)
                {
                    return new ApiResponse(404, $"Exercise with ID {id} not found.");
                }

                existingExercise.IsDeleted = true;
                _unitOfWork.Repository<Exercise>().Update(existingExercise);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to delete the exercise.");
                }

                return new ApiResponse(200, "Exercise deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "An unexpected error occurred while deleting the exercise.", ex.Message);
            }
        }


        public async Task<IEnumerable<ExerciseDto>> GetDailyExercisesAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Invalid user ID for retrieving daily exercises.");
                return Enumerable.Empty<ExerciseDto>();
            }

            try
            {
                _logger.LogInformation("Retrieving daily exercises for user ID: {UserId}", userId);

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !(bool)user.IsProfileConfirmed)
                {
                    _logger.LogWarning("User with ID {UserId} not found or profile not confirmed.", userId);
                    return Enumerable.Empty<ExerciseDto>();
                }

                // جلب الـ Membership بتاع الـ User
                var membershipSpec = new MonthlyMembershipWithRelationsSpecification(m => m.UserId == userId && m.IsActive);
                var membership = await _unitOfWork.Repository<Membership>().GetEntityWithSpecAsync(membershipSpec);
                if (membership == null)
                {
                    _logger.LogWarning("No active membership found for user ID {UserId}", userId);
                    return Enumerable.Empty<ExerciseDto>();
                }

                var today = DateTime.Today.DayOfWeek;
                var spec = new WorkoutPlanByDayAndUserSpecification(today, membership.Id);
                var workoutPlans = await _unitOfWork.Repository<WorkoutPlan>().GetAllWithSpecAsync(spec);

                if (!workoutPlans.Any())
                {
                    _logger.LogWarning("No workout plans found for user ID {UserId} on {Day}", userId, today);
                    return Enumerable.Empty<ExerciseDto>();
                }

                var exercises = workoutPlans.SelectMany(wp => wp.Exercises)
                    .Where(e => !e.IsDeleted)
                    .Distinct();
                var exerciseDtos = _mapper.Map<IEnumerable<ExerciseDto>>(exercises);

                _logger.LogInformation("Retrieved {Count} daily exercises for user ID: {UserId}", exerciseDtos.Count(), userId);
                return exerciseDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving daily exercises for user ID: {UserId}", userId);
                throw new ApplicationException($"Failed to retrieve daily exercises: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ExerciseDto>> SearchExercisesAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return Enumerable.Empty<ExerciseDto>();
            }

            try
            {
                var spec = new ExerciseBySearchTermSpecification(searchTerm);
                var exercises = await _unitOfWork.Repository<Exercise>().GetAllWithSpecAsync(spec);
                var exerciseDtos = _mapper.Map<IEnumerable<ExerciseDto>>(exercises);

                return exerciseDtos;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to search exercises: {ex.Message}", ex);
            }
        }

        public async Task<ExerciseDto> GetExerciseAsync(int id)
        {
            try
            {
                var spec = new ExerciseByIdSpecification(id);
                var exercise = await _unitOfWork.Repository<Exercise>().GetEntityWithSpecAsync(spec);
                if (exercise == null)
                {
                    _logger.LogWarning("Exercise with ID {Id} not found.", id);
                    return null;
                }

                var exerciseDto = _mapper.Map<ExerciseDto>(exercise);
                return exerciseDto;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve exercise: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ExerciseDto>> GetFavoritesAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Enumerable.Empty<ExerciseDto>();
            }

            try
            {
                var spec = new UserFavoriteExerciseByUserSpecification(userId);
                var favorites = await _unitOfWork.Repository<UserFavoriteExercise>().GetEntityWithSpecAsync(spec);

                if (favorites == null || !favorites.Exercises.Any())
                {
                    return Enumerable.Empty<ExerciseDto>();
                }

                var exercises = favorites.Exercises.Where(e => !e.IsDeleted).Distinct();
                var exerciseDtos = _mapper.Map<IEnumerable<ExerciseDto>>(exercises);


                return exerciseDtos;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve favorite exercises: {ex.Message}", ex);
            }
        }



        public async Task<ApiResponse> AddToFavoritesAsync(int exerciseId, string userId)
        {
            if (exerciseId <= 0 || string.IsNullOrEmpty(userId))
            {
                return new ApiResponse(400, "Invalid exercise ID or user ID.");
            }

            try
            {
                var exercise = await _unitOfWork.Repository<Exercise>().GetByIdAsync(exerciseId);
                if (exercise == null || exercise.IsDeleted)
                {
                    return new ApiResponse(404, $"Exercise with ID {exerciseId} not found.");
                }
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse(404, $"User with ID {userId} not found.");
                }

                var spec = new UserFavoriteExerciseByUserAndExerciseSpecification(userId);
                var existingFavorite = await _unitOfWork.Repository<UserFavoriteExercise>().GetEntityWithSpecAsync(spec);

                if (existingFavorite == null)
                {
                    existingFavorite = new UserFavoriteExercise { UserId = userId };

                    await _unitOfWork.Repository<UserFavoriteExercise>().Add(existingFavorite);
                    await _unitOfWork.Complete();
                }

                if (existingFavorite.Exercises.Any(e => e.Id == exerciseId))
                {
                    return new ApiResponse(400, "Exercise is already in favorites.");
                }

                existingFavorite.Exercises.Add(exercise);
                _unitOfWork.Repository<UserFavoriteExercise>().Update(existingFavorite);

                var result = await _unitOfWork.Complete();

                if (result <= 0)
                {
                    return new ApiExceptionResponse(500, "Failed to add exercise to favorites");
                }

                return new ApiResponse(200, "Exercise added to favorites successfully");
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An error occurred while adding to favorites", ex.Message);
            }
        }

        public async Task<ApiResponse> RemoveFromFavoritesAsync(int exerciseId, string userId)
        {
            if (exerciseId <= 0 || string.IsNullOrEmpty(userId))
            {
                return new ApiResponse(400, "Invalid exercise ID or user ID.");
            }

            try
            {
                var spec = new UserFavoriteExerciseByUserAndExerciseSpecification(userId);
                var favorite = await _unitOfWork.Repository<UserFavoriteExercise>().GetEntityWithSpecAsync(spec);
                if (favorite == null)
                {
                    return new ApiResponse(404, "No Favorites  for this user.");
                }

                var exerciseToRemove = favorite.Exercises.FirstOrDefault(e => e.Id == exerciseId);
                if (exerciseToRemove == null)
                {
                    return new ApiResponse(404, "Exercise not found in favorites.");
                }

                favorite.Exercises.Remove(exerciseToRemove);
                _unitOfWork.Repository<UserFavoriteExercise>().Update(favorite);

                var result = await _unitOfWork.Complete();

                if (result <= 0)
                {
                    return new ApiExceptionResponse(500, "Failed to remove exercise from favorites");
                }

                return new ApiResponse(200, "Exercise removed from favorites successfully");
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An error occurred while removing from favorites", ex.Message);
            }
        }

        public async Task<IEnumerable<ExerciseDto>> GetExercisesByCategoryAsync(int categoryId)
        {
            try
            {
                var spec = new ExerciseByCategorySpecification(categoryId);
                var exercises = await _unitOfWork.Repository<Exercise>().GetAllWithSpecAsync(spec);
                var exerciseDtos = _mapper.Map<IEnumerable<ExerciseDto>>(exercises);
                _logger.LogInformation("Retrieved {Count} exercises for category ID: {CategoryId}", exerciseDtos.Count(), categoryId);
                return exerciseDtos;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve exercises: {ex.Message}", ex);
            }
        }

    }
}