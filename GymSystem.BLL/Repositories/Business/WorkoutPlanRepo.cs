using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
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
    public class WorkoutPlanRepo : IWorkoutPlanRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<WorkoutPlanRepo> _logger;
        private readonly UserManager<AppUser> _userManager;

        public WorkoutPlanRepo(IUnitOfWork unitOfWork, IMapper mapper, ILogger<WorkoutPlanRepo> logger, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<ApiResponse> CreateWorkoutPlan(WorkoutPlanDto workoutPlanDto)
        {
            if (workoutPlanDto == null)
            {
                _logger.LogWarning("Workout plan data is null.");
                return new ApiResponse(400, "Workout plan data cannot be null.");
            }

            try
            {
                _logger.LogInformation("Creating new workout plan: {PlanName} for day: {DayOfWeek}", workoutPlanDto.PlanName, workoutPlanDto.DayOfWeek);

                if (!string.IsNullOrEmpty(workoutPlanDto.TrainerId))
                {
                    var trainer = await _userManager.FindByIdAsync(workoutPlanDto.TrainerId);
                    if (trainer == null)
                    {
                        _logger.LogWarning("Trainer with ID {TrainerId} not found.", workoutPlanDto.TrainerId);
                        return new ApiResponse(404, "Trainer not found.");
                    }
                }

                var workoutPlanEntity = _mapper.Map<WorkoutPlan>(workoutPlanDto);
                workoutPlanEntity.IsDeleted = false;
                workoutPlanEntity.DayOfWeek = workoutPlanDto.DayOfWeek;

                if (workoutPlanDto.Image != null)
                {
                    workoutPlanEntity.ImageUrl = "default-image-url"; // Placeholder، تحتاج Logic لرفع الصور
                }

                await _unitOfWork.Repository<WorkoutPlan>().Add(workoutPlanEntity);
                var result = await _unitOfWork.Complete();

                if (result <= 0)
                {
                    _logger.LogError("Failed to save workout plan: {PlanName}", workoutPlanDto.PlanName);
                    return new ApiExceptionResponse(500, "Failed to create workout plan");
                }

                _logger.LogInformation("Workout plan {PlanName} created successfully with ID: {Id}", workoutPlanDto.PlanName, workoutPlanEntity.Id);
                return new ApiResponse(201, "Workout plan created successfully", _mapper.Map<WorkoutPlanDto>(workoutPlanEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workout plan: {PlanName}", workoutPlanDto?.PlanName);
                return new ApiExceptionResponse(500, "An error occurred while creating workout plan", ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateWorkoutPlan(int id, WorkoutPlanDto workoutPlanDto)
        {
            if (id <= 0 || workoutPlanDto == null)
            {
                _logger.LogWarning("Invalid workout plan ID {Id} or data.", id);
                return new ApiResponse(400, "Invalid workout plan ID or data.");
            }

            try
            {
                _logger.LogInformation("Updating workout plan with ID: {Id} to day: {DayOfWeek}", id, workoutPlanDto.DayOfWeek);

                var existingPlan = await _unitOfWork.Repository<WorkoutPlan>().GetByIdAsync(id);
                if (existingPlan == null || existingPlan.IsDeleted)
                {
                    _logger.LogWarning("Workout plan with ID {Id} not found or deleted.", id);
                    return new ApiResponse(404, $"Workout plan with ID {id} not found.");
                }

                if (!string.IsNullOrEmpty(workoutPlanDto.TrainerId))
                {
                    var trainer = await _userManager.FindByIdAsync(workoutPlanDto.TrainerId);
                    if (trainer == null)
                    {
                        _logger.LogWarning("Trainer with ID {TrainerId} not found.", workoutPlanDto.TrainerId);
                        return new ApiResponse(404, "Trainer not found.");
                    }
                }

                _mapper.Map(workoutPlanDto, existingPlan);
                existingPlan.DayOfWeek = workoutPlanDto.DayOfWeek;

                if (workoutPlanDto.Image != null)
                {
                    existingPlan.ImageUrl = "updated-image-url"; // Placeholder، تحتاج Logic لرفع الصور
                }

                _unitOfWork.Repository<WorkoutPlan>().Update(existingPlan);
                var result = await _unitOfWork.Complete();

                if (result <= 0)
                {
                    _logger.LogError("Failed to update workout plan with ID: {Id}", id);
                    return new ApiExceptionResponse(500, "Failed to update workout plan");
                }

                _logger.LogInformation("Workout plan with ID {Id} updated successfully", id);
                return new ApiResponse(200, "Workout plan updated successfully", _mapper.Map<WorkoutPlanDto>(existingPlan));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workout plan with ID: {Id}", id);
                return new ApiExceptionResponse(500, "An error occurred while updating workout plan", ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteWorkoutPlan(int workoutPlanId)
        {
            if (workoutPlanId <= 0)
            {
                _logger.LogWarning("Invalid workout plan ID: {WorkoutPlanId}", workoutPlanId);
                return new ApiResponse(400, "Invalid workout plan ID.");
            }

            try
            {
                _logger.LogInformation("Deleting workout plan with ID: {WorkoutPlanId}", workoutPlanId);

                var workoutPlan = await _unitOfWork.Repository<WorkoutPlan>().GetByIdAsync(workoutPlanId);
                if (workoutPlan == null || workoutPlan.IsDeleted)
                {
                    _logger.LogWarning("Workout plan with ID {WorkoutPlanId} not found or deleted.", workoutPlanId);
                    return new ApiResponse(404, $"Workout plan with ID {workoutPlanId} not found.");
                }

                workoutPlan.IsDeleted = true;
                _unitOfWork.Repository<WorkoutPlan>().Update(workoutPlan);
                var result = await _unitOfWork.Complete();

                if (result <= 0)
                {
                    _logger.LogError("Failed to delete workout plan with ID: {WorkoutPlanId}", workoutPlanId);
                    return new ApiExceptionResponse(500, "Failed to delete workout plan");
                }

                _logger.LogInformation("Workout plan with ID {WorkoutPlanId} deleted successfully", workoutPlanId);
                return new ApiResponse(200, "Workout plan deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workout plan with ID: {WorkoutPlanId}", workoutPlanId);
                return new ApiExceptionResponse(500, "An error occurred while deleting workout plan", ex.Message);
            }
        }

        public async Task<WorkoutPlanDto> GetWorkoutPlan(int workoutPlanId)
        {
            if (workoutPlanId <= 0)
            {
                _logger.LogWarning("Invalid workout plan ID: {WorkoutPlanId}", workoutPlanId);
                return null;
            }

            try
            {
                _logger.LogInformation("Retrieving workout plan with ID: {WorkoutPlanId}", workoutPlanId);

                var spec = new WorkoutPlanByIdSpecification(workoutPlanId);
                var workoutPlan = await _unitOfWork.Repository<WorkoutPlan>().GetEntityWithSpecAsync(spec);
                if (workoutPlan == null)
                {
                    _logger.LogWarning("Workout plan with ID {WorkoutPlanId} not found.", workoutPlanId);
                    return null;
                }

                var workoutPlanDto = _mapper.Map<WorkoutPlanDto>(workoutPlan);
                workoutPlanDto.Exercises = _mapper.Map<IEnumerable<ExerciseDto>>(workoutPlan.Exercises);
                _logger.LogInformation("Workout plan retrieved successfully with ID: {WorkoutPlanId}", workoutPlanId);
                return workoutPlanDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workout plan with ID: {WorkoutPlanId}", workoutPlanId);
                throw new ApplicationException($"Failed to retrieve workout plan: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<WorkoutPlanDto>> GetWorkoutPlans()
        {
            try
            {
                _logger.LogInformation("Retrieving all workout plans.");

                var spec = new AllWorkoutPlansSpecification();
                var workoutPlans = await _unitOfWork.Repository<WorkoutPlan>().GetAllWithSpecAsync(spec);
                var workoutPlanDtos = _mapper.Map<IEnumerable<WorkoutPlanDto>>(workoutPlans);

                foreach (var dto in workoutPlanDtos)
                {
                    var plan = workoutPlans.FirstOrDefault(w => w.Id == dto.WorkoutPlanId);
                    if (plan != null)
                    {
                        dto.Exercises = _mapper.Map<IEnumerable<ExerciseDto>>(plan.Exercises);
                    }
                }

                _logger.LogInformation("Retrieved {Count} workout plans.", workoutPlanDtos.Count());
                return workoutPlanDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workout plans.");
                throw new ApplicationException($"Failed to retrieve workout plans: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<WorkoutPlanDto>> GetWorkoutPlansByDay(DayOfWeek dayOfWeek)
        {
            try
            {
                _logger.LogInformation("Retrieving workout plans for day: {DayOfWeek}", dayOfWeek);

                var spec = new WorkoutPlansByDaySpecification(dayOfWeek);
                var workoutPlans = await _unitOfWork.Repository<WorkoutPlan>().GetAllWithSpecAsync(spec);
                var workoutPlanDtos = _mapper.Map<IEnumerable<WorkoutPlanDto>>(workoutPlans);

                foreach (var dto in workoutPlanDtos)
                {
                    var plan = workoutPlans.FirstOrDefault(w => w.Id == dto.WorkoutPlanId);
                    if (plan != null)
                    {
                        dto.Exercises = _mapper.Map<IEnumerable<ExerciseDto>>(plan.Exercises);
                    }
                }

                _logger.LogInformation("Retrieved {Count} workout plans for day: {DayOfWeek}", workoutPlanDtos.Count(), dayOfWeek);
                return workoutPlanDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workout plans for day: {DayOfWeek}", dayOfWeek);
                throw new ApplicationException($"Failed to retrieve workout plans: {ex.Message}", ex);
            }
        }
    }
}