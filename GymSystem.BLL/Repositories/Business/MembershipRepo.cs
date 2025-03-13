using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.MonthlyMembership;
using GymSystem.BLL.Dtos.User;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.MonthlyMembershipWithRelationsSpeci;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Enums.Business;
using GymSystem.DAL.Entities.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Transactions;

namespace GymSystem.BLL.Repositories.Business
{
    public class MembershipRepository : IMembershipRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MembershipRepository> _logger;
        private readonly IUserService _userService;
        private readonly IUserCodeGenerator _userCodeGenerator;

        public MembershipRepository(
            IUnitOfWork unitOfWork,
            IUserService userService,
            IMapper mapper,
            IUserCodeGenerator userCodeGenerator,
            ILogger<MembershipRepository> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userCodeGenerator = userCodeGenerator ?? throw new ArgumentNullException(nameof(userCodeGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Retrieval Methods

        public async Task<IEnumerable<MonthlyMembershipViewDto>> GetAllAsync(SpecPrams specParams = null)
        {
            try
            {

                ISpecification<MonthlyMembership> spec = specParams != null
                    ? new MonthlyMembershipWithFiltersSpecification(specParams)
                    : new MonthlyMembershipWithRelationsSpecification();

                var memberships = await _unitOfWork.Repository<MonthlyMembership>().GetAllWithSpecAsync(spec);
                var membershipDtos = _mapper.Map<IEnumerable<MonthlyMembershipViewDto>>(memberships);

                foreach (var dto in membershipDtos)
                {
                    var membership = memberships.First(m => m.Id == dto.Id);
                    dto.UserName = membership.User?.DisplayName;
                    dto.UserCode = membership.User?.UserCode;
                }

                return membershipDtos;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to retrieve memberships.", ex);
            }
        }

        public async Task<MonthlyMembershipViewDto> GetByIdAsync(int id)
        {
            GuardAgainstInvalidId(id, "Membership ID");

            var spec = new MonthlyMembershipWithRelationsSpecification(m => m.Id == id);
            var membership = await _unitOfWork.Repository<MonthlyMembership>().GetEntityWithSpecAsync(spec);

            return membership == null ? null : _mapper.Map<MonthlyMembershipViewDto>(membership);
        }

        public async Task<IEnumerable<MonthlyMembershipViewDto>> GetActiveMembershipsAsync()
        {
            var spec = new MonthlyMembershipWithRelationsSpecification(m => m.IsActive);
            var memberships = await _unitOfWork.Repository<MonthlyMembership>().GetAllWithSpecAsync(spec);
            return MapMembershipsWithUserDetails(memberships);
        }

        public async Task<IEnumerable<MonthlyMembershipViewDto>> GetSuspendedMembershipsAsync()
        {
            var spec = new MonthlyMembershipWithRelationsSpecification(m => !m.IsActive);
            var memberships = await _unitOfWork.Repository<MonthlyMembership>().GetAllWithSpecAsync(spec);
            return MapMembershipsWithUserDetails(memberships);
        }

        #endregion

        #region CRUD Operations

        public async Task<ApiResponse> CreateAsync(MonthlyMembershipCreateDto membershipDto)
        {
            GuardAgainstNullInput(membershipDto, "Membership data");

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var plan = await GetPlanOrFail(membershipDto.PlanId);
                var user = await HandleUserCreationOrUpdate(membershipDto, plan.Id);

                if (await HasActiveMembership(user.Id))
                {
                    return new ApiResponse(409, "User already has an active membership.");
                }

                var membership = MapAndConfigureMembership(membershipDto, user, plan);
                await RecordFinancialTransaction(membership, TransactionType.Payment);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    transactionScope.Dispose();
                    return new ApiResponse(500, "Failed to save the monthly membership to the database.");
                }

                transactionScope.Complete();
                return new ApiResponse(201, "Monthly membership created successfully", _mapper.Map<MonthlyMembershipViewDto>(membership));
            }
        }

        public async Task<ApiResponse> UpdateAsync(int id, MonthlyMembershipUpdateDto membershipDto)
        {
            GuardAgainstInvalidId(id, "Membership ID");
            GuardAgainstNullInput(membershipDto, "Membership data");

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var membership = await GetMembershipOrFail(id);
                var user = await _userService.FindByIdAsync(membership.UserId) ?? throw new SecurityException("User not found.");

                if (await UpdateUserIfChanged(user, membershipDto))
                {
                    var updateResult = await _userService.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        transactionScope.Dispose();
                        return new ApiResponse(500, "Failed to update user information.");
                    }
                }

                _mapper.Map(membershipDto, membership);
                _unitOfWork.Repository<MonthlyMembership>().Update(membership);

                var saveResult = await _unitOfWork.Complete();
                if (saveResult <= 0)
                {
                    transactionScope.Dispose();
                    return new ApiResponse(500, "Failed to update user details.");
                }

                transactionScope.Complete();
                return new ApiResponse(200, "User details updated successfully", _mapper.Map<MonthlyMembershipViewDto>(membership));
            }
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            GuardAgainstInvalidId(id, "Membership ID");

            var membership = await GetMembershipOrFail(id);
            _unitOfWork.Repository<MonthlyMembership>().Delete(membership);

            var result = await _unitOfWork.Complete();
            return result > 0
                ? new ApiResponse(200, "Monthly membership deleted successfully")
                : new ApiResponse(500, "Failed to delete the monthly membership from the database.");
        }

        public async Task<ApiResponse> StopMembershipAsync(StopMembershipDto stopMembershipDto, string currentUserId)
        {
            GuardAgainstInvalidInput(stopMembershipDto, currentUserId);

            var membership = await GetActiveMembershipByUserCode(stopMembershipDto.UserCode);
            ValidateStopConditions(membership, stopMembershipDto);

            membership.EndDate = membership.EndDate.AddDays(stopMembershipDto.NumberOfDays);
            membership.StopDate = DateTime.UtcNow.AddDays(stopMembershipDto.NumberOfDays);
            membership.LastStopDate = DateTime.UtcNow;
            _unitOfWork.Repository<MonthlyMembership>().Update(membership);

            var saveResult = await _unitOfWork.Complete();
            return saveResult > 0
                ? new ApiResponse(200, "Membership stopped successfully")
                : new ApiResponse(500, "Failed to persist the stop membership operation.");
        }

        public async Task<ApiResponse> RenewMembershipAsync(MonthlyMembershipRenewDto renewDto)
        {
            GuardAgainstInvalidId(renewDto.MembershipId, "Membership ID");

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var membership = await GetMembershipOrFail(renewDto.MembershipId);
                var newPlan = await GetPlanOrFail(renewDto.PlanId);

                membership.Plan = newPlan;
                membership.EndDate = membership.EndDate.AddDays(newPlan.DurationDays);
                membership.IsActive = true;
                membership.HaveDays = newPlan.DurationDays;
                _unitOfWork.Repository<MonthlyMembership>().Update(membership);

                await RecordFinancialTransaction(membership, TransactionType.Payment);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    transactionScope.Dispose();
                    return new ApiResponse(500, "Failed to renew the membership in the database.");
                }

                transactionScope.Complete();
                return new ApiResponse(200, "Membership renewed successfully", _mapper.Map<MonthlyMembershipViewDto>(membership));
            }
        }

        #endregion

        #region User Profile Management

        public async Task<UserProfileDto> GetUserProfileAsync(string userId)
        {
            var user = await _userService.FindByIdAsync(userId);
            return user == null ? null : _mapper.Map<UserProfileDto>(user);
        }

        public async Task<ApiResponse> UpdateProfileAsync(string userId, UpdateProfileDto profileDto)
        {
            var user = await _userService.FindByIdAsync(userId) ?? throw new SecurityException("User not found.");
            _mapper.Map(profileDto, user);

            var result = await _userService.UpdateAsync(user);
            return result.Succeeded
                ? new ApiResponse(200, "Profile updated successfully", _mapper.Map<UserProfileDto>(user))
                : new ApiResponse(400, "Failed to update profile.", result.Errors.Select(e => e.Description));
        }

        public async Task<ApiResponse> UpdateGoalAsync(string userId, UpdateGoalDto goalDto)
        {
            var user = await _userService.FindByIdAsync(userId) ?? throw new SecurityException("User not found.");
            user.Goal = goalDto.Goal;

            var result = await _userService.UpdateAsync(user);
            return result.Succeeded
                ? new ApiResponse(200, "Goal updated successfully")
                : new ApiResponse(400, "Failed to update goal.", result.Errors.Select(e => e.Description));
        }

        public async Task<ApiResponse> UpdateLevelAsync(string userId, UpdateLevelDto levelDto)
        {
            var user = await _userService.FindByIdAsync(userId) ?? throw new SecurityException("User not found.");
            user.FitnessLevel = levelDto.FitnessLevel;

            var result = await _userService.UpdateAsync(user);
            return result.Succeeded
                ? new ApiResponse(200, "Fitness level updated successfully")
                : new ApiResponse(400, "Failed to update fitness level.", result.Errors);
        }

        public async Task<ApiResponse> ConfirmProfileAsync(string userId)
        {
            var user = await _userService.FindByIdAsync(userId) ?? throw new SecurityException("User not found.");
            user.IsProfileConfirmed = true;

            var result = await _userService.UpdateAsync(user);
            return result.Succeeded
                ? new ApiResponse(200, "Profile confirmed successfully")
                : new ApiResponse(400, "Failed to confirm profile.", result.Errors);
        }

        #endregion

        #region Private Helper Methods

        private void GuardAgainstInvalidId(int id, string entityName)
        {
            if (id <= 0) throw new ArgumentException($"{entityName} must be a positive integer.", entityName);
        }

        private void GuardAgainstNullInput(object input, string inputName)
        {
            if (input == null) throw new ArgumentNullException(inputName, $"{inputName} cannot be null.");
        }

        private void GuardAgainstInvalidInput(StopMembershipDto stopMembershipDto, string currentUserId)
        {
            if (stopMembershipDto == null) throw new ArgumentNullException(nameof(stopMembershipDto), "Stop membership data cannot be null.");
            if (string.IsNullOrWhiteSpace(stopMembershipDto.UserCode)) throw new ArgumentException("UserCode cannot be null or empty.", nameof(stopMembershipDto.UserCode));
            if (stopMembershipDto.NumberOfDays <= 0) throw new ArgumentException("Number of days must be greater than zero.", nameof(stopMembershipDto.NumberOfDays));
            if (string.IsNullOrWhiteSpace(currentUserId)) throw new SecurityException("Authenticated user ID is required.");
            if (!Guid.TryParse(currentUserId, out _)) throw new SecurityException("Current user ID must be a valid GUID.");
        }

        private async Task<MonthlyMembership> GetMembershipOrFail(int id)
        {
            var spec = new MonthlyMembershipWithRelationsSpecification(m => m.Id == id);
            var membership = await _unitOfWork.Repository<MonthlyMembership>().GetEntityWithSpecAsync(spec);
            if (membership == null) throw new KeyNotFoundException($"Membership with ID {id} not found.");
            return membership;
        }

        private async Task<Plan> GetPlanOrFail(int planId)
        {
            var plan = await _unitOfWork.Repository<Plan>().GetByIdAsync(planId);
            if (plan == null) throw new KeyNotFoundException("Plan not found.");
            return plan;
        }

        private async Task<AppUser> HandleUserCreationOrUpdate(MonthlyMembershipCreateDto membershipDto, int planId)
        {
            var existingUser = await _userService.FindByEmailAsync(membershipDto.UserEmail);
            if (existingUser == null)
            {
                var userCode = await _userCodeGenerator.GenerateUserCodeAsync(planId, await _userService.CountAsync());
                existingUser = new AppUser
                {
                    DisplayName = membershipDto.UserName,
                    UserName = membershipDto.UserName,
                    Email = membershipDto.UserEmail,
                    PhoneNumber = membershipDto.phoneNumber,
                    UserRole = 1,
                    EmailConfirmed = true,
                    UserCode = userCode
                };

                var result = await _userService.CreateAsync(existingUser, "Default@123");
                if (!result.Succeeded) throw new InvalidOperationException("Failed to create user.");
            }
            else if (string.IsNullOrEmpty(existingUser.UserCode))
            {
                existingUser.UserCode = await _userCodeGenerator.GenerateUserCodeAsync(planId, await _userService.CountAsync());
                await _userService.UpdateAsync(existingUser);
            }

            return existingUser;
        }

        private async Task<bool> HasActiveMembership(string userId)
        {
            var spec = new BaseSpecification<MonthlyMembership>(m => m.UserId == userId && m.IsActive);
            return await _unitOfWork.Repository<MonthlyMembership>().GetEntityWithSpecAsync(spec) != null;
        }

        private MonthlyMembership MapAndConfigureMembership(MonthlyMembershipCreateDto dto, AppUser user, Plan plan)
        {
            var membership = _mapper.Map<MonthlyMembership>(dto);
            membership.User = user;
            membership.UserId = user.Id;
            membership.UserName = user.UserName;
            membership.UserCode = user.UserCode;
            membership.UserEmail = user.Email;
            membership.PhoneNumber = user.PhoneNumber;
            membership.Plan = plan;
            membership.EndDate = membership.StartDate.AddDays(plan.DurationDays);
            membership.HaveDays = plan.DurationDays;
            membership.IsActive = true;
            return membership;
        }

        private async Task RecordFinancialTransaction(MonthlyMembership membership, TransactionType transactionType)
        {
            var transaction = new FinancialTransaction
            {
                TransactionType = transactionType,
                Amount = membership.Plan.Price,
                TransactionDate = DateTime.UtcNow,
                Description = $"Membership payment for UserCode: {membership.UserCode}",
                CreatedByUserId = membership.UserId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.Repository<FinancialTransaction>().Add(transaction);
        }

        private IEnumerable<MonthlyMembershipViewDto> MapMembershipsWithUserDetails(IEnumerable<MonthlyMembership> memberships)
        {
            var membershipDtos = _mapper.Map<IEnumerable<MonthlyMembershipViewDto>>(memberships);
            foreach (var dto in membershipDtos)
            {
                var membership = memberships.First(m => m.Id == dto.Id);
                dto.UserName = membership.User?.DisplayName;
                dto.UserCode = membership.User?.UserCode;
            }
            return membershipDtos;
        }

        private async Task<MonthlyMembership> GetActiveMembershipByUserCode(string userCode)
        {
            var spec = new BaseSpecification<MonthlyMembership>(m => m.UserCode == userCode && m.IsActive);
            var membership = await _unitOfWork.Repository<MonthlyMembership>().GetEntityWithSpecAsync(spec);
            if (membership == null) throw new KeyNotFoundException($"Active membership for UserCode {userCode} not found.");
            return membership;
        }

        private void ValidateStopConditions(MonthlyMembership membership, StopMembershipDto stopMembershipDto)
        {
            if (membership.LastStopDate.HasValue && membership.LastStopDate.Value > DateTime.UtcNow.AddMonths(-1))
                throw new InvalidOperationException("You can only stop your membership once per month.");

            if (membership.StopDate.HasValue && membership.StopDate.Value > DateTime.UtcNow)
                throw new InvalidOperationException($"Membership is already stopped until {membership.StopDate.Value:yyyy-MM-dd}.");
        }

        private async Task<bool> UpdateUserIfChanged(AppUser user, MonthlyMembershipUpdateDto membershipDto)
        {
            bool hasChanges = false;

            if (!string.IsNullOrEmpty(membershipDto.UserName) && user.UserName != membershipDto.UserName)
            {
                user.UserName = membershipDto.UserName;
                user.DisplayName = membershipDto.UserName;
                hasChanges = true;
            }

            if (!string.IsNullOrEmpty(membershipDto.UserEmail) && user.Email != membershipDto.UserEmail)
            {
                user.Email = membershipDto.UserEmail;
                user.NormalizedEmail = membershipDto.UserEmail.ToUpper();
                hasChanges = true;
            }

            if (!string.IsNullOrEmpty(membershipDto.PhoneNumber) && user.PhoneNumber != membershipDto.PhoneNumber)
            {
                user.PhoneNumber = membershipDto.PhoneNumber;
                hasChanges = true;
            }

            return hasChanges;
        }

        #endregion
    }

    //// Helper class for input validation
    //internal static class Guard
    //{
    //    public static void AgainstInvalidId(int id, string entityName)
    //    {
    //        if (id <= 0) throw new ArgumentException($"{entityName} must be a positive integer.", entityName);
    //    }

    //    public static void AgainstNullInput(object input, string inputName)
    //    {
    //        if (input == null) throw new ArgumentNullException(inputName, $"{inputName} cannot be null.");
    //    }
    //}
}