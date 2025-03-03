using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.MonthlyMembershipWithRelationsSpeci;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;


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
                _logger.LogWarning("Invalid membership ID: {Id}", membershipId);
                return new ApiResponse(400, "Membership ID must be a positive integer.");
            }

            try
            {
                _logger.LogInformation("Attempting to renew membership with ID: {Id}", membershipId);

                var spec = new MonthlyMembershipWithRelationsSpecification(m => m.Id == membershipId && !m.IsDeleted);
                var membership = await _unitOfWork.Repository<MonthlyMembership>().GetEntityWithSpecAsync(spec);
                if (membership == null)
                {
                    _logger.LogWarning("Membership with ID {Id} not found.", membershipId);
                    return new ApiResponse(404, $"Membership with ID {membershipId} not found.");
                }

                membership.EndDate = membership.EndDate.AddDays(30);
                membership.IsActive = true;
                _unitOfWork.Repository<MonthlyMembership>().Update(membership);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("Failed to renew membership with ID: {Id}", membershipId);
                    return new ApiResponse(500, "Failed to renew the membership in the database.");
                }

                var updatedDto = _mapper.Map<MonthlyMembershipDto>(membership);
                var user = await _userManager.FindByIdAsync(membership.UserId);
                var classEntity = await _unitOfWork.Repository<Class>().GetByIdAsync(membership.ClassId);
                updatedDto.UserName = user?.DisplayName;
                updatedDto.ClassName = classEntity?.ClassName;

                _logger.LogInformation("Membership with ID {Id} renewed successfully.", membershipId);
                return new ApiResponse(200, "Membership renewed successfully", updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renewing membership with ID: {Id}", membershipId);
                return new ApiExceptionResponse(500, $"Failed to renew membership: {ex.Message}");
            }
        }
    }

}