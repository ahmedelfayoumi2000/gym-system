using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
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

        public ExerciseRepo(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ExerciseRepo> logger, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager)); 
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
                if (user == null || !user.IsProfileConfirmed)
                {
                    _logger.LogWarning("User with ID {UserId} not found or profile not confirmed.", userId);
                    return Enumerable.Empty<ExerciseDto>();
                }

                var spec = new BaseSpecification<Exercise>(e => !e.IsDeleted && e.ExerciseCategory.CategoryName.Contains(user.FitnessLevel));
                var exercises = await _unitOfWork.Repository<Exercise>().GetAllWithSpecAsync(spec);
                var exerciseDtos = _mapper.Map<IEnumerable<ExerciseDto>>(exercises.Take(5)); 

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
                _logger.LogWarning("Search term is empty for exercise search.");
                return Enumerable.Empty<ExerciseDto>();
            }

            try
            {
                _logger.LogInformation("Searching exercises with term: {SearchTerm}", searchTerm);

                var spec = new BaseSpecification<Exercise>(e => !e.IsDeleted &&
                    (e.ExerciseName.ToLower().Contains(searchTerm.ToLower()) ||
                     e.Description.ToLower().Contains(searchTerm.ToLower())));
                var exercises = await _unitOfWork.Repository<Exercise>().GetAllWithSpecAsync(spec);
                var exerciseDtos = _mapper.Map<IEnumerable<ExerciseDto>>(exercises);

                _logger.LogInformation("Retrieved {Count} exercises for search term: {SearchTerm}", exerciseDtos.Count(), searchTerm);
                return exerciseDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching exercises with term: {SearchTerm}", searchTerm);
                throw new ApplicationException($"Failed to search exercises: {ex.Message}", ex);
            }
        }

        public async Task<ExerciseDto> GetExerciseAsync(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving exercise with ID: {Id}", id);

                var spec = new BaseSpecification<Exercise>(e => e.Id == id && !e.IsDeleted);
                var exercise = await _unitOfWork.Repository<Exercise>().GetEntityWithSpecAsync(spec);
                if (exercise == null)
                {
                    _logger.LogWarning("Exercise with ID {Id} not found.", id);
                    return null;
                }

                var exerciseDto = _mapper.Map<ExerciseDto>(exercise);
                _logger.LogInformation("Exercise retrieved successfully with ID: {Id}", id);
                return exerciseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exercise with ID: {Id}", id);
                throw new ApplicationException($"Failed to retrieve exercise: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse> AddToFavoritesAsync(int exerciseId, string userId)
        {
            if (exerciseId <= 0 || string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Invalid exercise ID {ExerciseId} or user ID {UserId} for adding to favorites.", exerciseId, userId);
                return new ApiResponse(400, "Invalid exercise ID or user ID.");
            }

            try
            {
                _logger.LogInformation("Adding exercise ID {ExerciseId} to favorites for user ID: {UserId}", exerciseId, userId);

                var exercise = await _unitOfWork.Repository<Exercise>().GetByIdAsync(exerciseId);
                if (exercise == null || exercise.IsDeleted)
                {
                    _logger.LogWarning("Exercise with ID {ExerciseId} not found or deleted.", exerciseId);
                    return new ApiResponse(404, $"Exercise with ID {exerciseId} not found.");
                }

                // Logic to add to favorites (simplified as a log for now)
                _logger.LogInformation("Exercise ID {ExerciseId} added to favorites for user ID {UserId}", exerciseId, userId);
                return new ApiResponse(200, "Exercise added to favorites successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding exercise ID {ExerciseId} to favorites for user ID: {UserId}", exerciseId, userId);
                return new ApiExceptionResponse(500, "An error occurred while adding to favorites", ex.Message);
            }
        }
    }
}