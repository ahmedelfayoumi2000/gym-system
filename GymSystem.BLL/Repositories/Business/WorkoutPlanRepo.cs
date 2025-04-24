using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.MembershipSpec;
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
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<ApiResponse> CreateWorkoutPlan(WorkoutPlanDto workoutPlanDto)
        {
            try
            {
                _logger.LogInformation("Trainer {TrainerId} creating workout plan for Membership {MembershipId} on {DayOfWeek}",
                    workoutPlanDto.TrainerId, workoutPlanDto.MembershipId, workoutPlanDto.DayOfWeek);

                var trainer = await _userManager.FindByIdAsync(workoutPlanDto.TrainerId);
                if (trainer == null || !await _userManager.IsInRoleAsync(trainer, "Trainer"))
                    return new ApiResponse(403, "Invalid or non-Trainer user.");

                var membershipSpec = new MonthlyMembershipWithRelationsSpecification(m => m.Id == workoutPlanDto.MembershipId);
                var membership = await _unitOfWork.Repository<Membership>().GetEntityWithSpecAsync(membershipSpec);
                if (membership == null)
                    return new ApiResponse(404, "Membership not found.");

                var existingPlanSpec = new WorkoutPlansByDaySpecification(workoutPlanDto.DayOfWeek, null, workoutPlanDto.MembershipId);
                var existingPlans = await _unitOfWork.Repository<WorkoutPlan>().GetAllWithSpecAsync(existingPlanSpec);
                if (existingPlans.Any())
                    return new ApiResponse(409, $"A workout plan already exists for this member on {workoutPlanDto.DayOfWeek}.");

                var workoutPlanEntity = _mapper.Map<WorkoutPlan>(workoutPlanDto);
                workoutPlanEntity.IsDeleted = false;
                workoutPlanEntity.Exercises = new List<Exercise>();

                await _unitOfWork.Repository<WorkoutPlan>().Add(workoutPlanEntity);
                var result = await _unitOfWork.Complete();

                if (result <= 0)
                    return new ApiExceptionResponse(500, "Failed to create workout plan");

                return new ApiResponse(201, "Workout plan created successfully", _mapper.Map<WorkoutPlanDto>(workoutPlanEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workout plan");
                return new ApiExceptionResponse(500, "Error creating workout plan", ex.Message);
            }
        }

        public async Task<ApiResponse> AddExerciseToWorkoutPlan(int workoutPlanId, int exerciseId, int membershipId, string trainerId)
        {
            try
            {
                _logger.LogInformation("Trainer {TrainerId} adding exercise {ExerciseId} to workout plan {WorkoutPlanId} for Membership {MembershipId}",
                    trainerId, exerciseId, workoutPlanId, membershipId);

                var spec = new WorkoutPlanByIdSpecification(workoutPlanId);
                var workoutPlan = await _unitOfWork.Repository<WorkoutPlan>().GetEntityWithSpecAsync(spec);
                if (workoutPlan == null)
                    return new ApiResponse(404, $"Workout plan with ID {workoutPlanId} not found.");

                if (workoutPlan.MembershipId != membershipId)
                    return new ApiResponse(400, $"Workout plan {workoutPlanId} does not belong to Membership {membershipId}.");

                if (workoutPlan.TrainerId != trainerId)
                    return new ApiResponse(403, "You can only add exercises to your own workout plans.");

                var exercise = await _unitOfWork.Repository<Exercise>().GetByIdAsync(exerciseId);
                if (exercise == null || exercise.IsDeleted)
                    return new ApiResponse(404, $"Exercise with ID {exerciseId} not found.");

                if (workoutPlan.Exercises.Any(e => e.Id == exerciseId))
                    return new ApiResponse(409, $"Exercise with ID {exerciseId} is already in the workout plan.");

                workoutPlan.Exercises.Add(exercise);
                _unitOfWork.Repository<WorkoutPlan>().Update(workoutPlan);
                var result = await _unitOfWork.Complete();

                if (result <= 0)
                    return new ApiExceptionResponse(500, "Failed to add exercise to workout plan");

                return new ApiResponse(200, "Exercise added to workout plan successfully", _mapper.Map<WorkoutPlanDto>(workoutPlan));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding exercise {ExerciseId} to workout plan {WorkoutPlanId}", exerciseId, workoutPlanId);
                return new ApiExceptionResponse(500, "Error adding exercise to workout plan", ex.Message);
            }
        }

        public async Task<ApiResponse> RemoveExerciseFromWorkoutPlan(int workoutPlanId, int exerciseId, int membershipId, string trainerId)
        {
            try
            {
                _logger.LogInformation("Trainer {TrainerId} removing exercise {ExerciseId} from workout plan {WorkoutPlanId} for Membership {MembershipId}",
                    trainerId, exerciseId, workoutPlanId, membershipId);

                var spec = new WorkoutPlanByIdSpecification(workoutPlanId);
                var workoutPlan = await _unitOfWork.Repository<WorkoutPlan>().GetEntityWithSpecAsync(spec);
                if (workoutPlan == null)
                    return new ApiResponse(404, $"Workout plan with ID {workoutPlanId} not found.");

                if (workoutPlan.MembershipId != membershipId)
                    return new ApiResponse(400, $"Workout plan {workoutPlanId} does not belong to Membership {membershipId}.");

                if (workoutPlan.TrainerId != trainerId)
                    return new ApiResponse(403, "You can only remove exercises from your own workout plans.");

                var exerciseToRemove = workoutPlan.Exercises.FirstOrDefault(e => e.Id == exerciseId);
                if (exerciseToRemove == null)
                    return new ApiResponse(404, $"Exercise with ID {exerciseId} not found in workout plan {workoutPlanId}.");

                workoutPlan.Exercises.Remove(exerciseToRemove);
                _unitOfWork.Repository<WorkoutPlan>().Update(workoutPlan);
                var result = await _unitOfWork.Complete();

                if (result <= 0)
                    return new ApiExceptionResponse(500, "Failed to remove exercise from workout plan");

                return new ApiResponse(200, "Exercise removed from workout plan successfully", _mapper.Map<WorkoutPlanDto>(workoutPlan));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing exercise {ExerciseId} from workout plan {WorkoutPlanId}", exerciseId, workoutPlanId);
                return new ApiExceptionResponse(500, "Error removing exercise from workout plan", ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateWorkoutPlan(int id, WorkoutPlanDto workoutPlanDto, string trainerId)
        {
            try
            {
                var spec = new WorkoutPlanByIdSpecification(id);
                var existingPlan = await _unitOfWork.Repository<WorkoutPlan>().GetEntityWithSpecAsync(spec);
                if (existingPlan == null)
                    return new ApiResponse(404, $"Workout plan with ID {id} not found.");

                if (existingPlan.TrainerId != trainerId)
                    return new ApiResponse(403, "You can only update your own workout plans.");

                workoutPlanDto.TrainerId = trainerId;

                var membershipSpec = new MonthlyMembershipWithRelationsSpecification(m => m.Id == workoutPlanDto.MembershipId);
                var membership = await _unitOfWork.Repository<Membership>().GetEntityWithSpecAsync(membershipSpec);
                if (membership == null)
                    return new ApiResponse(404, "Membership not found.");

                if (existingPlan.DayOfWeek != workoutPlanDto.DayOfWeek)
                {
                    var existingPlanSpec = new WorkoutPlansByDaySpecification(workoutPlanDto.DayOfWeek, null, workoutPlanDto.MembershipId);
                    var existingPlans = await _unitOfWork.Repository<WorkoutPlan>().GetAllWithSpecAsync(existingPlanSpec);
                    if (existingPlans.Any(p => p.Id != id))
                        return new ApiResponse(409, $"A workout plan already exists for this member on {workoutPlanDto.DayOfWeek}.");
                }

                _mapper.Map(workoutPlanDto, existingPlan);
                _unitOfWork.Repository<WorkoutPlan>().Update(existingPlan);
                var result = await _unitOfWork.Complete();

                if (result <= 0)
                    return new ApiExceptionResponse(500, "Failed to update workout plan");

                return new ApiResponse(200, "Workout plan updated successfully", _mapper.Map<WorkoutPlanDto>(existingPlan));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workout plan ID: {Id}", id);
                return new ApiExceptionResponse(500, "Error updating workout plan", ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteWorkoutPlan(int id, string trainerId)
        {
            try
            {
                var spec = new WorkoutPlanByIdSpecification(id);
                var workoutPlan = await _unitOfWork.Repository<WorkoutPlan>().GetEntityWithSpecAsync(spec);
                if (workoutPlan == null)
                    return new ApiResponse(404, $"Workout plan with ID {id} not found.");

                if (workoutPlan.TrainerId != trainerId)
                    return new ApiResponse(403, "You can only delete your own workout plans.");

                workoutPlan.IsDeleted = true;
                _unitOfWork.Repository<WorkoutPlan>().Update(workoutPlan);
                var result = await _unitOfWork.Complete();

                if (result <= 0)
                    return new ApiExceptionResponse(500, "Failed to delete workout plan");

                return new ApiResponse(200, "Workout plan deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workout plan ID: {Id}", id);
                return new ApiExceptionResponse(500, "Error deleting workout plan", ex.Message);
            }
        }

        public async Task<IEnumerable<WorkoutPlanDto>> GetWorkoutPlansForMember(int membershipId, string trainerId)
        {
            try
            {
                var membershipSpec = new MonthlyMembershipWithRelationsSpecification(m => m.Id == membershipId);
                var membership = await _unitOfWork.Repository<Membership>().GetEntityWithSpecAsync(membershipSpec);
                if (membership == null)
                    throw new Exception("Membership not found.");

                var spec = new WorkoutPlansByMembershipForTrainerSpecification(membershipId, trainerId);
                var workoutPlans = await _unitOfWork.Repository<WorkoutPlan>().GetAllWithSpecAsync(spec);
                var workoutPlanDtos = _mapper.Map<IEnumerable<WorkoutPlanDto>>(workoutPlans);

                foreach (var dto in workoutPlanDtos)
                {
                    var plan = workoutPlans.FirstOrDefault(w => w.Id == dto.WorkoutPlanId);
                    dto.Exercises = _mapper.Map<IEnumerable<ExerciseDto>>(plan?.Exercises);
                }

                return workoutPlanDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workout plans for Membership {MembershipId}", membershipId);
                throw;
            }
        }

        public async Task<IEnumerable<WorkoutPlanDto>> GetMemberWorkoutPlans(string userId)
        {
            try
            {
                var membershipSpec = new MonthlyMembershipWithRelationsSpecification(m => m.UserId == userId && m.IsActive);
                var membership = await _unitOfWork.Repository<Membership>().GetEntityWithSpecAsync(membershipSpec);
                if (membership == null)
                {
                    _logger.LogWarning("No active membership found for user {UserId}", userId);
                    throw new Exception("No active membership found for this user.");
                }

                var spec = new WorkoutPlansForMemberSpecification(membership.Id);
                var workoutPlans = await _unitOfWork.Repository<WorkoutPlan>().GetAllWithSpecAsync(spec);
                var workoutPlanDtos = _mapper.Map<IEnumerable<WorkoutPlanDto>>(workoutPlans);

                foreach (var dto in workoutPlanDtos)
                {
                    var plan = workoutPlans.FirstOrDefault(w => w.Id == dto.WorkoutPlanId);
                    dto.Exercises = _mapper.Map<IEnumerable<ExerciseDto>>(plan?.Exercises);
                }

                _logger.LogInformation("Retrieved {Count} workout plans for user {UserId}", workoutPlanDtos.Count(), userId);
                return workoutPlanDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workout plans for user {UserId}", userId);
                throw;
            }
        }

        public async Task<ApiResponse> GetWorkoutPlanById(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving workout plan with ID {WorkoutPlanId}", id);

                var spec = new WorkoutPlanByIdSpecification(id);
                var workoutPlan = await _unitOfWork.Repository<WorkoutPlan>().GetEntityWithSpecAsync(spec);

                if (workoutPlan == null)
                    return new ApiResponse(404, $"Workout plan with ID {id} not found.");

                var workoutPlanDto = _mapper.Map<WorkoutPlanDto>(workoutPlan);
                workoutPlanDto.Exercises = _mapper.Map<IEnumerable<ExerciseDto>>(workoutPlan.Exercises);

                return new ApiResponse(200, "Workout plan retrieved successfully", workoutPlanDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workout plan with ID {WorkoutPlanId}", id);
                return new ApiExceptionResponse(500, "Error retrieving workout plan", ex.Message);
            }
        }
    }
}