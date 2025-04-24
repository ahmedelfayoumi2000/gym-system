using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IExerciseRepo
    {
        Task<ExerciseDto> AddExerciseAsync(ExerciseDto exerciseDto);
        Task<ApiResponse> GenerateExercisesForUserAsync(int userId);
        Task<ExerciseDto> UpdateExerciseAsync(int id, ExerciseDto exerciseDto);
        public Task<ApiResponse> DeleteExercise(int id);
        Task<IEnumerable<ExerciseDto>> GetDailyExercisesAsync(string userId);
        Task<IEnumerable<ExerciseDto>> SearchExercisesAsync(string searchTerm);
        Task<ExerciseDto> GetExerciseAsync(int id);
        Task<ApiResponse> AddToFavoritesAsync(int exerciseId, string userId);
        Task<IEnumerable<ExerciseDto>> GetExercisesByCategoryAsync(int categoryId);
        Task<ApiResponse> RemoveFromFavoritesAsync(int exerciseId, string userId);
        Task<IEnumerable<ExerciseDto>> GetFavoritesAsync(string userId);

    }
}
