using GymSystem.BLL.Dtos.MonthlyMembership;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security;
using System.Threading.Tasks;
using GymSystem.BLL.Dtos.User;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Services;
using Microsoft.AspNetCore.Identity;
using GymSystem.DAL.Entities.Identity;
using GymSystem.API.Helpers;
using GymSystem.DAL.Entities;

namespace GymSystem.API.Controllers
{

    //إضافة مشترك
    public class MembershipController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMembershipRepo _membershipRepo;

        public MembershipController(
                                     IUserService userService,
                                     UserManager<AppUser> userManager,
                                     IMembershipRepo membershipRepo)
        {
            _userService = userService;
            _userManager = userManager;
            _membershipRepo = membershipRepo ?? throw new ArgumentNullException(nameof(membershipRepo));
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllMemberships()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiExceptionResponse(401, "User is not authenticated. Please log in."));
                }

                var user = await _userService.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiExceptionResponse(404, $"User with ID {userId} not found in the database."));
                }

                var memberships = await _membershipRepo.GetAllAsync();
                var total = memberships.Count();
                var activemember = memberships.Count(memberships => memberships.IsActive);
                var Susbendmember = memberships.Count(memberships => !memberships.IsActive);
                var roles = await _userManager.GetRolesAsync(user);

                var responsedto = new HomeMembership()
                {
                    UserName = user.DisplayName,
                    Roles = roles.ToList(),
                    Members = memberships,
                    TotalMembers = total,
                    TotalActiveMembers = activemember,
                    TotalSusbendMembers = Susbendmember

                };
                return Ok(new ApiResponse(200, "memberships retrieved successfully", responsedto));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "an error occurred while retrieving memberships", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMembershipById(int id)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Membership ID must be a positive integer."));
            }


            try
            {
                var membership = await _membershipRepo.GetByIdAsync(id);
                if (membership == null)
                {
                    return NotFound(new ApiResponse(404, $"Membership with ID {id} not found"));
                }

                return Ok(new ApiResponse(200, "Membership retrieved successfully", membership));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while retrieving membership with ID {id}", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateMembership([FromBody] MonthlyMembershipCreateDto membershipDto)
        {
            if (!ModelState.IsValid || membershipDto == null)
            {
                return BadRequest(CreateValidationError("Invalid membership data"));
            }

            try
            {
                var response = await _membershipRepo.CreateAsync(membershipDto);
                return HandleApiResponse(response, StatusCodes.Status201Created);
            }
            catch (ApplicationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while creating the membership", ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An unexpected error occurred", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMembership(int id, [FromBody] MonthlyMembershipUpdateDto membershipDto)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Membership ID must be a positive integer."));
            }

            if (!ModelState.IsValid || membershipDto == null)
            {
                return BadRequest(CreateValidationError("Invalid membership data"));
            }


            try
            {
                var response = await _membershipRepo.UpdateAsync(id, membershipDto);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while updating membership with ID {id}", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMembership(int id)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Membership ID must be a positive integer."));
            }

            try
            {
                var response = await _membershipRepo.DeleteAsync(id);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while deleting membership with ID {id}", ex.Message));
            }
        }

        /// <summary>
        /// Retrieves all active memberships (IsActive = true).
        /// </summary>
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetActiveMemberships([FromQuery] SpecPrams specParams)
        {
            try
            {
                specParams.IsActive = true;
                var data = await _membershipRepo.GetActiveMembershipsAsync(specParams);
                var totalCount = data.Count();


                return Ok(new ApiResponse(200, "Active memberships retrieved successfully", new Pagination<MonthlyMembershipViewDto>(specParams.PageIndex, specParams.PageSize, totalCount, data)));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving active memberships", ex.Message));
            }
        }

        /// <summary>
        /// Retrieves all suspended memberships (IsActive = false).
        /// </summary>
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet("suspended")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSuspendedMemberships([FromQuery] SpecPrams specParams)
        {
            try
            {
                specParams.IsActive = false;
                var data = await _membershipRepo.GetSuspendedMembershipsAsync(specParams);
                var totalCount = data.Count();


                return Ok(new ApiResponse(200, "Suspended memberships retrieved successfully", new Pagination<MonthlyMembershipViewDto>(specParams.PageIndex, specParams.PageSize, totalCount, data)));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving suspended memberships", ex.Message));
            }
        }

        /// <summary>
        /// Renews a membership by extending its end date and setting it to active.
        /// </summary>
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost("renew")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RenewMembership(MonthlyMembershipRenewDto renewDto)
        {
            if (!IsValidId(renewDto.MembershipId))
            {
                return BadRequest(CreateValidationError("Membership ID must be a positive integer."));
            }

            try
            {
                var response = await _membershipRepo.RenewMembershipAsync(renewDto);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while renewing membership with ID {renewDto.MembershipId}", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost("stop-membership")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StopMembershipAsync([FromBody] StopMembershipDto stopMembershipDto)
        {
            try
            {

                if (stopMembershipDto == null || string.IsNullOrWhiteSpace(stopMembershipDto.UserCode))
                {
                    return BadRequest(new ApiResponse(400, "Stop membership data or UserCode cannot be null or empty."));
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    return Unauthorized(new ApiResponse(401, "User authentication required."));
                }

                var response = await _membershipRepo.StopMembershipAsync(stopMembershipDto, currentUserId);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(400, ex.Message));
            }
            catch (SecurityException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(403, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiExceptionResponse(500, "An unexpected error occurred during stop membership.", ex.Message));
            }
        }

        [Authorize]
        [HttpGet("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var profile = await _membershipRepo.GetUserProfileAsync(userId);
                if (profile == null)
                {
                    return NotFound(new ApiResponse(404, "User profile not found"));
                }

                return Ok(new ApiResponse(200, "Profile retrieved successfully", profile));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving the profile", ex.Message));
            }
        }



        [Authorize]
        [HttpPut("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto profileDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(CreateValidationError("Invalid profile data"));
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var response = await _membershipRepo.UpdateProfileAsync(userId, profileDto);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while updating the profile", ex.Message));
            }
        }



        [Authorize]
        [HttpPut("profile/goal")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateGoal([FromBody] UpdateGoalDto goalDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(CreateValidationError("Invalid goal data"));
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var response = await _membershipRepo.UpdateGoalAsync(userId, goalDto);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while updating the goal", ex.Message));
            }
        }



        [Authorize]
        [HttpPut("profile/level")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLevel([FromBody] UpdateLevelDto levelDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(CreateValidationError("Invalid fitness level data"));
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var response = await _membershipRepo.UpdateLevelAsync(userId, levelDto);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while updating the fitness level", ex.Message));
            }
        }



        [Authorize]
        [HttpGet("profile/confirm")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var response = await _membershipRepo.ConfirmProfileAsync(userId);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while confirming the profile", ex.Message));
            }
        }

        #region Private Helper Methods

        private bool IsValidId(int id) => id > 0;

        private ApiValidationErrorResponse CreateValidationError(string message)
        {
            return new ApiValidationErrorResponse
            {
                Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList(),
                StatusCode = 400,
                Message = message
            };
        }

        private IActionResult HandleApiResponse(ApiResponse response, int successStatusCode = StatusCodes.Status200OK)
        {
            return response.StatusCode switch
            {
                200 => Ok(response),
                201 => StatusCode(StatusCodes.Status201Created, response),
                400 => BadRequest(response),
                404 => NotFound(response),
                409 => Conflict(response),
                500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                _ => StatusCode(response.StatusCode ?? 500, response)
            };
        }

        #endregion
    }
}