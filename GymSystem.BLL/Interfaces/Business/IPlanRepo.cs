using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.plan;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Specifications;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IPlanRepo
    {
        //Task<IReadOnlyList<PlanDto>> GetAllAsync(SpecPrams specParams = null);
        Task<PlanDto> GetByIdAsync(int id);
        Task<ApiResponse> CreateAsync(PlanDto planDto);
        Task<ApiResponse> UpdateAsync(int id, PlanDto planDto);
        Task<ApiResponse> DeleteAsync(int id);

        //--------------
        Task<ApiResponse> GetPlans();
    }
}