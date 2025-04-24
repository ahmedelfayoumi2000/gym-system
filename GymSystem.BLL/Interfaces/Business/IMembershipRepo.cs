using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.MonthlyMembership;
using GymSystem.BLL.Dtos.User;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Specifications;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IMembershipRepo
    {
        Task<IEnumerable<MonthlyMembershipViewDto>> GetAllAsync(SpecPrams specParams = null);
        Task<MonthlyMembershipViewDto> GetByIdAsync(int id);
        Task<ApiResponse> CreateAsync(MonthlyMembershipCreateDto membershipDto);
        Task<ApiResponse> UpdateAsync(int id, MonthlyMembershipUpdateDto membershipDto);
        Task<ApiResponse> DeleteAsync(int id);
        Task<IEnumerable<MonthlyMembershipViewDto>> GetActiveMembershipsAsync(SpecPrams specParams = null);
        Task<IEnumerable<MonthlyMembershipViewDto>> GetSuspendedMembershipsAsync(SpecPrams specParams = null);
        Task<ApiResponse> RenewMembershipAsync(MonthlyMembershipRenewDto renewDto);
        Task<ApiResponse> StopMembershipAsync(StopMembershipDto stopMembershipDto, string currentUserId);

        //Mopile
        Task<UserProfileDto> GetUserProfileAsync(string userId);
        Task<ApiResponse> UpdateProfileAsync(string userId, UpdateProfileDto profileDto);
        Task<ApiResponse> UpdateGoalAsync(string userId, UpdateGoalDto goalDto);
        Task<ApiResponse> UpdateLevelAsync(string userId, UpdateLevelDto levelDto);
        Task<ApiResponse> ConfirmProfileAsync(string userId);
    }
}