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
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security;


namespace GymSystem.BLL.Repositories.Business
{
    public class MembershipRepository : IMembershipRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<MembershipRepository> _logger;

        public MembershipRepository(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IMapper mapper,
            ILogger<MembershipRepository> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<MonthlyMembershipDto>> GetAllAsync(SpecPrams specParams = null)
        {
            try
            {
                _logger.LogInformation("Retrieving all memberships with parameters: {@SpecParams}", specParams);

                ISpecification<MonthlyMembership> spec = specParams != null
                    ? new MonthlyMembershipWithFiltersSpecification(specParams)
                    : new MonthlyMembershipWithRelationsSpecification();

                var memberships = await _unitOfWork.Repository<MonthlyMembership>().GetAllWithSpecAsync(spec);
                var membershipDtos = _mapper.Map<IEnumerable<MonthlyMembershipDto>>(memberships);

                foreach (var dto in membershipDtos)
                {
                    var membership = memberships.First(m => m.Id == dto.Id);
                    dto.UserName = membership.User?.DisplayName;
                    dto.ClassName = membership.Class?.ClassName;
                }

                _logger.LogInformation("Retrieved {Count} memberships.", membershipDtos.Count());
                return membershipDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving memberships.");
                throw new ApplicationException($"Failed to retrieve memberships: {ex.Message}", ex);
            }
        }

        public async Task<MonthlyMembershipDto> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid membership ID: {Id}", id);
                return null;
            }

            try
            {
                _logger.LogInformation("Retrieving membership with ID: {Id}", id);

                var spec = new MonthlyMembershipWithRelationsSpecification(m => m.Id == id);
                var membership = await _unitOfWork.Repository<MonthlyMembership>().GetEntityWithSpecAsync(spec);
                if (membership == null)
                {
                    _logger.LogWarning("Membership with ID {Id} not found.", id);
                    return null;
                }

                var membershipDto = _mapper.Map<MonthlyMembershipDto>(membership);
                membershipDto.UserName = membership.User?.DisplayName;
                membershipDto.ClassName = membership.Class?.ClassName;

                _logger.LogInformation("Membership with ID {Id} retrieved successfully.", id);
                return membershipDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving membership with ID: {Id}", id);
                throw new ApplicationException($"Failed to retrieve membership: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse> CreateAsync(MonthlyMembershipDto membershipDto)
        {
            if (membershipDto == null)
            {
                _logger.LogWarning("Attempted to create a null MembershipDto.");
                return new ApiResponse(400, "Membership data cannot be null.");
            }

            try
            {
                _logger.LogInformation("Attempting to create membership for UserId: {UserId}, ClassId: {ClassId}",
                    membershipDto.UserId, membershipDto.ClassId);

                var spec = new BaseSpecification<MonthlyMembership>(m => m.UserId == membershipDto.UserId &&
                    m.ClassId == membershipDto.ClassId && !m.IsDeleted);
                var existingMembership = await _unitOfWork.Repository<MonthlyMembership>().GetEntityWithSpecAsync(spec);
                if (existingMembership != null)
                {
                    _logger.LogWarning("Membership for UserId {UserId} and ClassId {ClassId} already exists.",
                        membershipDto.UserId, membershipDto.ClassId);
                    return new ApiResponse(409, "User already has an active membership for this class.");
                }

                var membershipEntity = _mapper.Map<MonthlyMembership>(membershipDto);
                await _unitOfWork.Repository<MonthlyMembership>().Add(membershipEntity);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("Failed to save membership for UserId: {UserId}", membershipDto.UserId);
                    return new ApiResponse(500, "Failed to save the membership to the database.");
                }

                var createdDto = _mapper.Map<MonthlyMembershipDto>(membershipEntity);
                var user = await _userManager.FindByIdAsync(membershipEntity.UserId);
                var classEntity = await _unitOfWork.Repository<Class>().GetByIdAsync(membershipEntity.ClassId);
                createdDto.UserName = user?.DisplayName;
                createdDto.ClassName = classEntity?.ClassName;

                _logger.LogInformation("Membership created successfully with ID: {Id}", membershipEntity.Id);
                return new ApiResponse(201, "Membership created successfully", createdDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating membership for UserId: {UserId}", membershipDto.UserId);
                return new ApiExceptionResponse(500, $"Failed to create membership: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateAsync(int id, MonthlyMembershipDto membershipDto)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid membership ID: {Id}", id);
                return new ApiResponse(400, "Membership ID must be a positive integer.");
            }

            if (membershipDto == null)
            {
                _logger.LogWarning("Attempted to update membership with ID {Id} using null MembershipDto.", id);
                return new ApiResponse(400, "Membership data cannot be null.");
            }

            try
            {
                _logger.LogInformation("Attempting to update membership with ID: {Id}", id);

                var spec = new BaseSpecification<MonthlyMembership>(m => m.Id == id && !m.IsDeleted);
                var existingMembership = await _unitOfWork.Repository<MonthlyMembership>().GetEntityWithSpecAsync(spec);
                if (existingMembership == null)
                {
                    _logger.LogWarning("Membership with ID {Id} not found.", id);
                    return new ApiResponse(404, $"Membership with ID {id} not found.");
                }

                _mapper.Map(membershipDto, existingMembership);
                _unitOfWork.Repository<MonthlyMembership>().Update(existingMembership);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("Failed to update membership with ID: {Id}", id);
                    return new ApiResponse(500, "Failed to update the membership in the database.");
                }

                var updatedDto = _mapper.Map<MonthlyMembershipDto>(existingMembership);
                var user = await _userManager.FindByIdAsync(existingMembership.UserId);
                var classEntity = await _unitOfWork.Repository<Class>().GetByIdAsync(existingMembership.ClassId);
                updatedDto.UserName = user?.DisplayName;
                updatedDto.ClassName = classEntity?.ClassName;

                _logger.LogInformation("Membership with ID {Id} updated successfully.", id);
                return new ApiResponse(200, "Membership updated successfully", updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating membership with ID: {Id}", id);
                return new ApiExceptionResponse(500, $"Failed to update membership: {ex.Message}");
            }
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid membership ID: {Id}", id);
                return new ApiResponse(400, "Membership ID must be a positive integer.");
            }

            try
            {
                _logger.LogInformation("Attempting to delete membership with ID: {Id}", id);

                var spec = new BaseSpecification<MonthlyMembership>(m => m.Id == id && !m.IsDeleted);
                var membership = await _unitOfWork.Repository<MonthlyMembership>().GetEntityWithSpecAsync(spec);
                if (membership == null)
                {
                    _logger.LogWarning("Membership with ID {Id} not found or already deleted.", id);
                    return new ApiResponse(404, $"Membership with ID {id} not found.");
                }

                membership.IsDeleted = true;
                _unitOfWork.Repository<MonthlyMembership>().Update(membership);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("Failed to delete membership with ID: {Id}", id);
                    return new ApiResponse(500, "Failed to delete the membership from the database.");
                }

                _logger.LogInformation("Membership with ID {Id} deleted successfully.", id);
                return new ApiResponse(200, "Membership deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting membership with ID: {Id}", id);
                return new ApiExceptionResponse(500, $"Failed to delete membership: {ex.Message}");
            }
        }

        public async Task<IEnumerable<MonthlyMembershipDto>> GetActiveMembershipsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all active memberships.");

                var spec = new MonthlyMembershipWithRelationsSpecification(m => m.IsActive && !m.IsDeleted);
                var memberships = await _unitOfWork.Repository<MonthlyMembership>().GetAllWithSpecAsync(spec);
                var membershipDtos = _mapper.Map<IEnumerable<MonthlyMembershipDto>>(memberships);

                foreach (var dto in membershipDtos)
                {
                    var membership = memberships.First(m => m.Id == dto.Id);
                    dto.UserName = membership.User?.DisplayName;
                    dto.ClassName = membership.Class?.ClassName;
                }

                _logger.LogInformation("Retrieved {Count} active memberships.", membershipDtos.Count());
                return membershipDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active memberships.");
                throw new ApplicationException($"Failed to retrieve active memberships: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<MonthlyMembershipDto>> GetSuspendedMembershipsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all suspended memberships.");

                var spec = new MonthlyMembershipWithRelationsSpecification(m => !m.IsActive && !m.IsDeleted);
                var memberships = await _unitOfWork.Repository<MonthlyMembership>().GetAllWithSpecAsync(spec);
                var membershipDtos = _mapper.Map<IEnumerable<MonthlyMembershipDto>>(memberships);

                foreach (var dto in membershipDtos)
                {
                    var membership = memberships.First(m => m.Id == dto.Id);
                    dto.UserName = membership.User?.DisplayName;
                    dto.ClassName = membership.Class?.ClassName;
                }

                _logger.LogInformation("Retrieved {Count} suspended memberships.", membershipDtos.Count());
                return membershipDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving suspended memberships.");
                throw new ApplicationException($"Failed to retrieve suspended memberships: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse> RenewMembershipAsync(int membershipId)
        {
            if (membershipId <= 0)
            {
                return new ApiResponse(400, "Membership ID must be a positive integer.");
            }

            try
            {

                var spec = new MonthlyMembershipWithRelationsSpecification(m => m.Id == membershipId && !m.IsDeleted);
                var membership = await _unitOfWork.Repository<MonthlyMembership>().GetEntityWithSpecAsync(spec);
                if (membership == null)
                {
                    return new ApiResponse(404, $"Membership with ID {membershipId} not found.");
                }

                membership.EndDate = membership.EndDate.AddDays(30);
                membership.IsActive = true;
                _unitOfWork.Repository<MonthlyMembership>().Update(membership);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to renew the membership in the database.");
                }

                var updatedDto = _mapper.Map<MonthlyMembershipDto>(membership);
                var user = await _userManager.FindByIdAsync(membership.UserId);
                var classEntity = await _unitOfWork.Repository<Class>().GetByIdAsync(membership.ClassId);
                updatedDto.UserName = user?.DisplayName;
                updatedDto.ClassName = classEntity?.ClassName;

                return new ApiResponse(200, "Membership renewed successfully", updatedDto);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, $"Failed to renew membership: {ex.Message}");
            }
        }

        public async Task<UserProfileDto> GetUserProfileAsync(string userId)
        {
            try
            {

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return null;
                }

                var profile = _mapper.Map<UserProfileDto>(user);
                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile for user ID: {UserId}", userId);
                throw new ApplicationException($"Failed to retrieve profile: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse> UpdateProfileAsync(string userId, UpdateProfileDto profileDto)
        {
            try
            {

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse(404, "User not found.");
                }

                _mapper.Map(profileDto, user);
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return new ApiResponse(400, $"Failed to update profile for user ID {userId}.", result.Errors.Select(e => e.Description));
                }

                var updatedProfile = _mapper.Map<UserProfileDto>(user);
                return new ApiResponse(200, "Profile updated successfully", updatedProfile);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An error occurred while updating the profile", ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateGoalAsync(string userId, UpdateGoalDto goalDto)
        {
            try
            {

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse(404, $"User with ID {userId} not found.");
                }

                user.Goal = goalDto.Goal;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return new ApiResponse(400, $"Failed to update goal for user ID {userId}.", result.Errors.Select(e => e.Description));
                }

                return new ApiResponse(200, "Goal updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An error occurred while updating the goal", ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateLevelAsync(string userId, UpdateLevelDto levelDto)
        {
            try
            {
                _logger.LogInformation("Attempting to update fitness level for user ID: {UserId}", userId);

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", userId);
                    return new ApiResponse(404, "User not found.");
                }

                user.FitnessLevel = levelDto.FitnessLevel;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed to update fitness level for user ID {UserId}: {Errors}", userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return new ApiResponse(400, "Failed to update fitness level.", result.Errors);
                }

                _logger.LogInformation("Fitness level updated successfully for user ID: {UserId}", userId);
                return new ApiResponse(200, "Fitness level updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating fitness level for user ID: {UserId}", userId);
                return new ApiExceptionResponse(500, "An error occurred while updating the fitness level", ex.Message);
            }
        }

        public async Task<ApiResponse> ConfirmProfileAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Attempting to confirm profile for user ID: {UserId}", userId);

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", userId);
                    return new ApiResponse(404, "User not found.");
                }

                user.IsProfileConfirmed = true;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed to confirm profile for user ID {UserId}: {Errors}", userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return new ApiResponse(400, "Failed to confirm profile.", result.Errors);
                }

                _logger.LogInformation("Profile confirmed successfully for user ID: {UserId}", userId);
                return new ApiResponse(200, "Profile confirmed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming profile for user ID: {UserId}", userId);
                return new ApiExceptionResponse(500, "An error occurred while confirming the profile", ex.Message);
            }
        }

        public async Task<ApiResponse> StopMembershipAsync(StopMembershipDto stopMembershipDto, string currentUserId)
        {
            if (stopMembershipDto == null)
            {
                return new ApiResponse(400, "Stop membership data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(stopMembershipDto.UserCode))
            {
                return new ApiResponse(400, "UserCode cannot be null or empty.");
            }

            if (stopMembershipDto.NumberOfDays <= 0)
            {
                return new ApiResponse(400, "Number of days must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return new ApiResponse(401, "Authenticated user ID is required.");
            }

            try
            {

                if (!Guid.TryParse(currentUserId, out _))
                {
                    return new ApiResponse(401, "Current user ID must be a valid GUID.");
                }

                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    return new ApiResponse(401, $"Current user with ID {currentUserId} not found.");
                }

                var membershipRepo = _unitOfWork.Repository<Membership>();
                var membershipSpec = new BaseSpecification<Membership>(m => m.UserCode == stopMembershipDto.UserCode && !m.IsDeleted);
                var membership = await membershipRepo.GetEntityWithSpecAsync(membershipSpec);
                if (membership == null)
                {
                    return new ApiResponse(404, $"Membership for UserCode {stopMembershipDto.UserCode} not found.");
                }

                if (membership.StopDate.HasValue && membership.StopDate.Value > DateTime.UtcNow)
                {
                    return new ApiResponse(409, $"Membership for UserCode {stopMembershipDto.UserCode} is already stopped until {membership.StopDate}.");
                }

                var currentTime = DateTime.UtcNow;
                membership.EndDate = membership.EndDate.AddDays(stopMembershipDto.NumberOfDays); 
                membership.StopDate = currentTime.AddDays(stopMembershipDto.NumberOfDays); 
                membership.IsActive = false; // Deactivate membership
                membershipRepo.Update(membership);

                var saveResult = await _unitOfWork.Complete();
                if (saveResult <= 0)
                {
                    return new ApiResponse(500, "Failed to persist the stop membership operation.");
                }

                return new ApiResponse(200, "Membership stopped successfully.");
            }
            catch (Exception ex) when (ex is ArgumentException or SecurityException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An unexpected error occurred during stop membership.", ex.Message);
            }
        }
    }

}