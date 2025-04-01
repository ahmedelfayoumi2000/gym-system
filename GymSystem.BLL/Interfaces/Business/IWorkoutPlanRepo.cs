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
        Task<ApiResponse> UpdateWorkoutPlan(int id, WorkoutPlanDto workoutPlanDto);
        Task<ApiResponse> DeleteWorkoutPlan(int workoutPlanId);
        Task<WorkoutPlanDto> GetWorkoutPlan(int workoutPlanId);
        Task<IEnumerable<WorkoutPlanDto>> GetWorkoutPlans();
        Task<IEnumerable<WorkoutPlanDto>> GetWorkoutPlansByDay(DayOfWeek dayOfWeek);
    }
}