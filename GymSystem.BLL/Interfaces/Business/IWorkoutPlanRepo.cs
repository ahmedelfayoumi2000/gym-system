using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IWorkoutPlanRepo
    {
        Task<ApiResponse> CreateWorkoutPlan(WorkoutPlanDto workoutPlanDto);
        Task<ApiResponse> AddExerciseToWorkoutPlan(int workoutPlanId, int exerciseId, int membershipId, string trainerId);
        Task<ApiResponse> RemoveExerciseFromWorkoutPlan(int workoutPlanId, int exerciseId, int membershipId, string trainerId);
        Task<ApiResponse> UpdateWorkoutPlan(int id, WorkoutPlanDto workoutPlanDto, string trainerId);
        Task<ApiResponse> DeleteWorkoutPlan(int id, string trainerId);
        Task<IEnumerable<WorkoutPlanDto>> GetWorkoutPlansForMember(int membershipId, string trainerId);
        Task<IEnumerable<WorkoutPlanDto>> GetMemberWorkoutPlans(string userId);
        Task<ApiResponse> GetWorkoutPlanById(int id);
    }
}