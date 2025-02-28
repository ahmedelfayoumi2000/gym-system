using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Specifications;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IMonthlyMembershipRepo
    {
        Task<IReadOnlyList<MonthlyMembershipDto>> GetAllAsync(SpecPrams specParams = null);
        Task<MonthlyMembershipDto> GetByIdAsync(int id);
        Task<ApiResponse> CreateAsync(MonthlyMembershipDto membershipDto);
        Task<ApiResponse> UpdateAsync(int id, MonthlyMembershipDto membershipDto);
        Task<ApiResponse> DeleteAsync(int id);
    }
}