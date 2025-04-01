using GymSystem.API.DTOs.Trainer;
using GymSystem.BLL.Dtos.Trainer;
using GymSystem.BLL.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface ITrainerService
    {
        Task<TrainerDto> GetTrainerByIdAsync(string id);
        Task<IEnumerable<TrainerDto>> GetAllTrainersAsync();
        Task<TrainerDto> CreateTrainerAsync(CreateTrainerDto trainerDto);
        Task<bool> UpdateTrainerAsync(string id, UpdateTrainerDto trainerDto);
        Task<bool> DeleteTrainerAsync(string id);
        Task<ApiResponse> SuspendTrainerAsync(string id);
    }
}
