using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IMembershipRepo
    {
        Task<IEnumerable<MonthlyMembershipDto>> GetAllAsync(SpecPrams specParams = null);
        Task<MonthlyMembershipDto> GetByIdAsync(int id);
        Task<ApiResponse> CreateAsync(MonthlyMembershipDto membershipDto);
        Task<ApiResponse> UpdateAsync(int id, MonthlyMembershipDto membershipDto);
        Task<ApiResponse> DeleteAsync(int id);
        Task<IEnumerable<MonthlyMembershipDto>> GetActiveMembershipsAsync();
        Task<IEnumerable<MonthlyMembershipDto>> GetSuspendedMembershipsAsync();
        Task<ApiResponse> RenewMembershipAsync(int membershipId);
    }
}
