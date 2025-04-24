using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.Attendance;
using GymSystem.BLL.Dtos.MonthlyMembership;
using GymSystem.BLL.Dtos.User;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.MembershipSpec;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Enums.Business;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<MembershipRepository> _logger;
        private readonly IImageService _imageService;
        private readonly IUserService _userService;
        private readonly IUserCodeGenerator _userCodeGenerator;

        public MembershipRepository(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IUserService userService,
            IMapper mapper,
            IUserCodeGenerator userCodeGenerator,
            ILogger<MembershipRepository> logger,
            IImageService imageService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userManager = userManager;
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userCodeGenerator = userCodeGenerator ?? throw new ArgumentNullException(nameof(userCodeGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _imageService = imageService;
        }

        #region Retrieval Methods

        public async Task<IEnumerable<MonthlyMembershipViewDto>> GetAllAsync(SpecPrams specParams = null)
        {
            try
            {
                await UpdateExpiredMemberships();

                ISpecification<Membership> spec = new MonthlyMembershipWithRelationsSpecification();

                var memberships = await _unitOfWork.Repository<Membership>().GetAllWithSpecAsync(spec);

                return _mapper.Map<IEnumerable<MonthlyMembershipViewDto>>(memberships);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while retrieving memberships. Please try again later.", ex);
            }
        }

        public async Task<MonthlyMembershipViewDto> GetByIdAsync(int id)
        {
            GuardAgainstInvalidId(id, "Membership ID");

            var spec = new MonthlyMembershipWithRelationsSpecification(m => m.Id == id);
            var membership = await _unitOfWork.Repository<Membership>().GetEntityWithSpecAsync(spec);
            await UpdateExpiredMembership(membership);

            return membership == null ? null : _mapper.Map<MonthlyMembershipViewDto>(membership);
        }

        public async Task<IEnumerable<MonthlyMembershipViewDto>> GetActiveMembershipsAsync(SpecPrams specParams = null)
        {
            await UpdateExpiredMemberships();

            ISpecification<Membership> spec = new MonthlyMembershipWithFiltersSpecification(specParams);
            var memberships = await _unitOfWork.Repository<Membership>().GetAllWithSpecAsync(spec);
            return _mapper.Map<IEnumerable<MonthlyMembershipViewDto>>(memberships);
        }

        public async Task<IEnumerable<MonthlyMembershipViewDto>> GetSuspendedMembershipsAsync(SpecPrams specParams = null)
        {
            await UpdateExpiredMemberships();

            ISpecification<Membership> spec = new MonthlyMembershipWithFiltersSpecification(specParams);
            var memberships = await _unitOfWork.Repository<Membership>().GetAllWithSpecAsync(spec);
            return _mapper.Map<IEnumerable<MonthlyMembershipViewDto>>(memberships);
        }

        #endregion

        #region CRUD Operations

        public async Task<ApiResponse> CreateAsync(MonthlyMembershipCreateDto membershipDto)
        {
            GuardAgainstNullInput(membershipDto, "Membership data");

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var plan = await GetPlanOrFail(membershipDto.PlanId);
                    var user = await HandleUserCreationOrUpdate(membershipDto, plan.Id);

                    if (await HasActiveMembership(user.Id))
                    {
                        return new ApiResponse(409, "User already has an active membership.");
                    }

                    if (plan.ExpireDate.HasValue && plan.ExpireDate.Value.Date < DateTime.UtcNow.Date)
                    {
                        plan.HasOffer = false;
                        plan.DiscountedPrice = null;
                        plan.ExpireDate = null;

                        _unitOfWork.Repository<Plan>().Update(plan);
                    }

                    var membership = MapAndConfigureMembership(membershipDto, user, plan);
                    await _unitOfWork.Repository<Membership>().Add(membership);

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
                catch (InvalidOperationException ex)
                {
                    transactionScope.Dispose();
                    return new ApiResponse(400, ex.Message); 
                }
                catch (Exception ex)
                {
                    transactionScope.Dispose();
                    return new ApiResponse(500, "An error occurred while creating the membership. Please try again later.", ex);
                }
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
                await UpdateExpiredMembership(membership);

                _unitOfWork.Repository<Membership>().Update(membership);

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
            _unitOfWork.Repository<Membership>().Delete(membership);

            var result = await _unitOfWork.Complete();
            return result > 0
                ? new ApiResponse(200, "Monthly membership deleted successfully")
                : new ApiResponse(500, "Failed to delete the monthly membership from the database.");
        }

        public async Task<ApiResponse> StopMembershipAsync(StopMembershipDto stopMembershipDto, string currentUserId)
        {
            try
            {
                GuardAgainstInvalidInput(stopMembershipDto, currentUserId);

                var membership = await GetActiveMembershipByUserCode(stopMembershipDto.UserCode);
                await ValidateStopConditions(membership, stopMembershipDto);

                membership.EndDate = membership.EndDate.AddDays(stopMembershipDto.NumberOfDays);
                membership.StopDate = DateTime.UtcNow.AddDays(stopMembershipDto.NumberOfDays);
                membership.LastStopDate = DateTime.UtcNow;
                membership.IsActive = false;
                _unitOfWork.Repository<Membership>().Update(membership);

                var saveResult = await _unitOfWork.Complete();
                if (saveResult <= 0)
                {
                    return new ApiResponse(500, "Failed to persist the stop membership operation.");
                }

                return new ApiResponse(200, "Membership stopped successfully");
            }
            catch (InvalidOperationException ex)
            {
                return new ApiResponse(400, ex.Message);
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "An unexpected error occurred while stopping the membership.");
            }
        }

        public async Task<ApiResponse> RenewMembershipAsync(MonthlyMembershipRenewDto renewDto)
        {
            GuardAgainstInvalidId(renewDto.MembershipId, "Membership ID");

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var membership = await GetMembershipOrFail(renewDto.MembershipId);
                await UpdateExpiredMembership(membership);

                var newPlan = await GetPlanOrFail(renewDto.PlanId);

                if (newPlan.ExpireDate < DateTime.UtcNow || newPlan.HasOffer == false)
                {
                    newPlan.HasOffer = false;
                    newPlan.DiscountedPrice = null;
                    newPlan.ExpireDate = null;
                }

                membership.Plan = newPlan;
                membership.Plan.Price = newPlan.DiscountedPrice ?? newPlan.Price;
                membership.EndDate = membership.EndDate.AddDays(newPlan.DurationDays);
                membership.IsActive = true;
                membership.HaveDays = newPlan.DurationDays;
                _unitOfWork.Repository<Membership>().Update(membership);
                _unitOfWork.Repository<Plan>().Update(newPlan);

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
            try
            {
                var user = await _userService.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new Exception($"User with ID {userId} not found in the database.");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var UserProfile = _mapper.Map<UserProfileDto>(user);
                UserProfile.Roles = roles.ToList();

                return UserProfile;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateProfileAsync(string userId, UpdateProfileDto profileDto)
        {
            var user = await _userService.FindByIdAsync(userId) ?? throw new SecurityException("User not found.");



            _mapper.Map(profileDto, user);
            if (profileDto.Image != null)
            {

                if (!string.IsNullOrEmpty(user.ProfileImageName))
                {
                    await _imageService.DeleteImageAsync(user.ProfileImageName);
                }
                var uploadResult = await _imageService.UploadImageAsync(profileDto.Image);
                if (uploadResult.Item1 == 1)
                {
                    user.ProfileImageName = uploadResult.Item2;
                }
                else
                {
                    throw new ApplicationException($"Failed to Upload Image: {uploadResult.Item2}");
                }
            }

            var result = await _userService.UpdateAsync(user);
            return result.Succeeded
                ? new ApiResponse(200, "Profile updated successfully", _mapper.Map<UserProfileDto>(user))
                : new ApiResponse(400, "Failed to update profile.", result.Errors.Select(e => e.Description));
        }

        public async Task<ApiResponse> UpdateGoalAsync(string userId, UpdateGoalDto goalDto)
        {
            var user = await _userService.FindByIdAsync(userId) ?? throw new SecurityException("User not found.");
            if (user.Goal == goalDto.Goal)
                return new ApiResponse(200, "No changes made to Your Goal");
            user.Goal = goalDto.Goal;

            var result = await _userService.UpdateAsync(user);
            return result.Succeeded
                ? new ApiResponse(200, "Goal updated successfully")
                : new ApiResponse(400, "Failed to update goal.", result.Errors.Select(e => e.Description));
        }

        public async Task<ApiResponse> UpdateLevelAsync(string userId, UpdateLevelDto levelDto)
        {
            var user = await _userService.FindByIdAsync(userId) ?? throw new SecurityException("User not found.");
            if (user.FitnessLevel == levelDto.FitnessLevel)
                return new ApiResponse(200, "No changes made to fitness level");
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

        private async Task<bool> HandleMembershipStatus(Membership membership)
        {
            if (membership == null)
            {
                _logger.LogWarning("Membership provided for status update is null.");
                return false;
            }

            bool needsUpdate = false;

            if (membership.EndDate < DateTime.UtcNow && membership.IsActive)
            {
                _logger.LogWarning("Membership for UserCode {UserCode} has expired on {EndDate}.", membership.UserCode, membership.EndDate);
                membership.IsActive = false;
                needsUpdate = true;
            }

            if (membership.StopDate.HasValue)
            {
                if (membership.StopDate < DateTime.UtcNow)
                {
                    _logger.LogInformation("StopDate for UserCode {UserCode} has expired. Resetting StopDate to null.", membership.UserCode);
                    membership.StopDate = null;
                    if (membership.EndDate >= DateTime.UtcNow)
                    {
                        membership.IsActive = true;
                        needsUpdate = true;
                    }
                }
                else if (membership.StopDate > DateTime.UtcNow && membership.IsActive)
                {
                    _logger.LogInformation("Membership for UserCode {UserCode} is currently stopped until {StopDate}.", membership.UserCode, membership.StopDate);
                    membership.IsActive = false;
                    needsUpdate = true;
                }
            }

            if (membership.LastStopDate.HasValue && membership.LastStopDate.Value.Month == DateTime.UtcNow.Month)
            {
                _logger.LogInformation("Membership for UserCode {UserCode} was stopped this month. No further stop allowed.", membership.UserCode);
                return false;
            }

            if (needsUpdate)
            {
                _unitOfWork.Repository<Membership>().Update(membership);
                return true;
            }

            return false;
        }

        private async Task UpdateExpiredMemberships()
        {
            var spec = new MonthlyMembershipWithRelationsSpecification(m => m.IsActive);
            var memberships = await _unitOfWork.Repository<Membership>().GetAllWithSpecAsync(spec);

            if (!memberships.Any())
            {
                _logger.LogInformation("No memberships found to update.");
                return;
            }

            var expiredList = memberships.ToList();
            var updatedCount = 0;

            foreach (var membership in expiredList)
            {
                if (await HandleMembershipStatus(membership))
                {
                    updatedCount++;
                }
            }

            if (updatedCount > 0)
            {
                await _unitOfWork.Complete();
                _logger.LogInformation("Updated {Count} memberships.", updatedCount);
            }
        }

        private async Task UpdateExpiredMembership(Membership membership)
        {
            if (await HandleMembershipStatus(membership))
            {
                await _unitOfWork.Complete();
            }
        }

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

        private async Task<Membership> GetMembershipOrFail(int id)
        {
            var spec = new MonthlyMembershipWithRelationsSpecification(m => m.Id == id);
            var membership = await _unitOfWork.Repository<Membership>().GetEntityWithSpecAsync(spec);
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
            // Check by email
            var existingUserByEmail = await _userService.FindByEmailAsync(membershipDto.UserEmail);
            if (existingUserByEmail != null)
            {
                if (existingUserByEmail.PhoneNumber == membershipDto.phoneNumber)
                {
                    throw new InvalidOperationException($"A user with PhoneNumber '{membershipDto.phoneNumber}' and email '{membershipDto.UserEmail}' already exists.");
                }
                if (string.IsNullOrEmpty(existingUserByEmail.UserCode))
                {
                    existingUserByEmail.UserCode = await _userCodeGenerator.GenerateUserCodeAsync(planId, await _userService.CountAsync());
                    var updateResult = await _userService.UpdateAsync(existingUserByEmail);
                    if (!updateResult.Succeeded)
                    {
                        throw new InvalidOperationException("Failed to update user code: " + string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                    }
                }
                return existingUserByEmail;
            }

            // Check by PhoneNumber 
            var existingUserByName = await _userService.FindByPhoneNumberAsync(membershipDto.phoneNumber);
            if (existingUserByName != null)
            {
                throw new InvalidOperationException($"A user with PhoneNumber '{membershipDto.phoneNumber}' already exists.");
            }
            // Create new user if no conflicts
            var userCode = await _userCodeGenerator.GenerateUserCodeAsync(planId, await _userService.CountAsync());
            var newUser = new AppUser
            {
                DisplayName = membershipDto.UserName,
                UserName = membershipDto.UserEmail + $"{userCode}",
                Email = membershipDto.UserEmail,
                PhoneNumber = membershipDto.phoneNumber,
                UserRole = 1,
                EmailConfirmed = true,
                UserCode = userCode
            };

            var result = await _userService.CreateAsync(newUser, "Default@123");
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Failed to create user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            string roleName = newUser.UserRole == 1 ? "Member" : null;
            var roleResult = await _userService.AddToRoleAsync(newUser, roleName);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to assign role to user: " + string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }

            return newUser;
        }

        private async Task<bool> HasActiveMembership(string userId)
        {
            var spec = new MembershipByUserCodeAndActiveSpecification(userId);
            return await _unitOfWork.Repository<Membership>().GetEntityWithSpecAsync(spec) != null;
        }

        private Membership MapAndConfigureMembership(MonthlyMembershipCreateDto dto, AppUser user, Plan plan)
        {
            var membership = _mapper.Map<Membership>(dto);
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

        private async Task RecordFinancialTransaction(Membership membership, TransactionType transactionType)
        {
            var transaction = new FinancialTransaction
            {
                TransactionType = transactionType,
                Amount = membership.Plan.DiscountedPrice ?? membership.Plan.Price,
                TransactionDate = DateTime.UtcNow,
                Description = $"Membership payment for UserCode: {membership.UserCode}",
                CreatedByUserId = membership.UserId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.Repository<FinancialTransaction>().Add(transaction);
        }

        private IEnumerable<MonthlyMembershipViewDto> MapMembershipsWithUserDetails(IEnumerable<Membership> memberships)
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

        private async Task<Membership> GetActiveMembershipByUserCode(string userCode)
        {
            var spec = new MembershipByUserCodeAndActiveSpecification(userCode);
            var membership = await _unitOfWork.Repository<Membership>().GetEntityWithSpecAsync(spec);
            if (membership == null) throw new KeyNotFoundException($"Active membership for UserCode {userCode} not found.");
            if (!membership.IsActive)
            {
                throw new InvalidOperationException($"Membership for UserCode {userCode} is not active. Please subscribe to a new plan to continue.");
            }
            if (membership.EndDate < DateTime.UtcNow)
            {
                membership.IsActive = false;
                _unitOfWork.Repository<Membership>().Update(membership);

                throw new InvalidOperationException($"Your membership expired on {membership.EndDate:yyyy-MM-dd}. Please renew your plan.");
            }
            return membership;
        }

        private async Task ValidateStopConditions(Membership membership, StopMembershipDto stopMembershipDto)
        {
            GuardAgainstNullInput(membership, nameof(membership));
            GuardAgainstNullInput(stopMembershipDto, nameof(stopMembershipDto));

            if (membership.LastStopDate.HasValue && membership.LastStopDate.Value > DateTime.UtcNow.AddMonths(-1))
            {
                if (!membership.StopDate.HasValue || membership.StopDate < DateTime.UtcNow)
                {
                    membership.StopDate = null;
                    await HandleMembershipStatus(membership);
                    await _unitOfWork.Complete();
                }
                else
                {
                    throw new InvalidOperationException("You can only stop your membership once per month.");
                }
            }

            if (membership.StopDate.HasValue)
            {
                if (membership.StopDate < DateTime.UtcNow)
                {
                    membership.StopDate = null;
                    await HandleMembershipStatus(membership);
                    await _unitOfWork.Complete();
                }
                else
                {
                    throw new InvalidOperationException($"Membership is already stopped until {membership.StopDate.Value:yyyy-MM-dd}.");
                }
            }
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
}