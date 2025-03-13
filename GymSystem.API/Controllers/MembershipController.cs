using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.MonthlyMembership;
using GymSystem.BLL.Dtos.User;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GymSystem.API.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    public class MembershipController : BaseApiController
    {
        private readonly IMembershipRepo _membershipRepo;
        private readonly ILogger<MembershipController> _logger;

        public MembershipController(IMembershipRepo membershipRepo, ILogger<MembershipController> logger)
        {
            _membershipRepo = membershipRepo ?? throw new ArgumentNullException(nameof(membershipRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Retrieval Endpoints

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllMemberships([FromQuery] SpecPrams specParams = null)
        {
            try
            {
                var memberships = await _membershipRepo.GetAllAsync(specParams);
                return Ok(new ApiResponse(200, "Memberships retrieved successfully", memberships));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving memberships.", ex.Message));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMembershipById(int id)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Membership ID must be a positive integer."));
            }

            _logger.LogInformation("Retrieving membership with ID: {Id}", id);

            try
            {
                var membership = await _membershipRepo.GetByIdAsync(id);
                if (membership == null)
                {
                    return NotFound(new ApiResponse(404, $"Membership with ID {id} not found."));
                }

                return Ok(new ApiResponse(200, "Membership retrieved successfully", membership));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while retrieving membership with ID {id}.", ex.Message));
            }
        }

        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetActiveMemberships()
        {
            try
            {
                var memberships = await _membershipRepo.GetActiveMembershipsAsync();
                return Ok(new ApiResponse(200, "Active memberships retrieved successfully", memberships));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving active memberships.", ex.Message));
            }
        }

        [HttpGet("suspended")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSuspendedMemberships()
        {
            try
            {
                var memberships = await _membershipRepo.GetSuspendedMembershipsAsync();
                return Ok(new ApiResponse(200, "Suspended memberships retrieved successfully", memberships));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving suspended memberships.", ex.Message));
            }
        }

        #endregion

        #region CRUD Operations

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateMembership([FromBody] MonthlyMembershipCreateDto membershipDto)
        {
            if (!IsValidModel(membershipDto))
            {
                return BadRequest(CreateValidationError("Invalid membership data."));
            }


            try
            {
                var response = await _membershipRepo.CreateAsync(membershipDto);
                return HandleApiResponse(response, StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating membership for user: {UserEmail}", membershipDto.UserEmail);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while creating the membership.", ex.Message));
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMembership(int id, [FromBody] MonthlyMembershipUpdateDto membershipDto)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Membership ID must be a positive integer."));
            }

            if (!IsValidModel(membershipDto))
            {
                return BadRequest(CreateValidationError("Invalid membership data."));
            }

            try
            {
                var response = await _membershipRepo.UpdateAsync(id, membershipDto);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while updating membership with ID {id}.", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
                    new ApiExceptionResponse(500, $"An error occurred while deleting membership with ID {id}.", ex.Message));
            }
        }

        [HttpPost("renew")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RenewMembership([FromBody] MonthlyMembershipRenewDto renewDto)
        {
            if (renewDto == null || !IsValidId(renewDto.MembershipId))
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
                _logger.LogError(ex, "Error renewing membership with ID: {MembershipId}", renewDto.MembershipId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while renewing membership with ID {renewDto.MembershipId}.", ex.Message));
            }
        }

        [HttpPost("stop-membership")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StopMembershipAsync([FromBody] StopMembershipDto stopMembershipDto)
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

            try
            {
                var response = await _membershipRepo.StopMembershipAsync(stopMembershipDto, currentUserId);
                return HandleApiResponse(response);
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
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An unexpected error occurred during stop membership.", ex.Message));
            }
        }

        #endregion

        #region User Profile Management

        [Authorize]
        [HttpGet("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new ApiResponse(401, "User authentication required."));
            }

            try
            {
                var profile = await _membershipRepo.GetUserProfileAsync(userId);
                if (profile == null)
                {
                    return NotFound(new ApiResponse(404, "User profile not found."));
                }

                return Ok(new ApiResponse(200, "Profile retrieved successfully", profile));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving the profile.", ex.Message));
            }
        }



        [Authorize]
        [HttpPut("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto profileDto)
        {
            if (!IsValidModel(profileDto))
            {
                return BadRequest(CreateValidationError("Invalid profile data."));
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new ApiResponse(401, "User authentication required."));
            }

            try
            {
                var response = await _membershipRepo.UpdateProfileAsync(userId, profileDto);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while updating the profile.", ex.Message));
            }
        }

        [Authorize]
        [HttpPut("profile/goal")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateGoal([FromBody] UpdateGoalDto goalDto)
        {
            if (!IsValidModel(goalDto))
            {
                return BadRequest(CreateValidationError("Invalid goal data."));
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new ApiResponse(401, "User authentication required."));
            }

            try
            {
                var response = await _membershipRepo.UpdateGoalAsync(userId, goalDto);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while updating the goal.", ex.Message));
            }
        }

        [Authorize]
        [HttpPut("profile/level")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLevel([FromBody] UpdateLevelDto levelDto)
        {
            if (!IsValidModel(levelDto))
            {
                return BadRequest(CreateValidationError("Invalid fitness level data."));
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new ApiResponse(401, "User authentication required."));
            }

            try
            {
                var response = await _membershipRepo.UpdateLevelAsync(userId, levelDto);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while updating the fitness level.", ex.Message));
            }
        }


        [Authorize]
        [HttpPost("profile/confirm")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new ApiResponse(401, "User authentication required."));
            }

            try
            {
                var response = await _membershipRepo.ConfirmProfileAsync(userId);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while confirming the profile.", ex.Message));
            }
        }

        #endregion

        #region Private Helper Methods

        private bool IsValidId(int id) => id > 0;

        private bool IsValidModel<T>(T model) => model != null && ModelState.IsValid;

        private string GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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
                401 => Unauthorized(response),
                403 => StatusCode(StatusCodes.Status403Forbidden, response),
                404 => NotFound(response),
                409 => Conflict(response),
                500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                _ => StatusCode(response.StatusCode ?? 500, response)
            };
        }

        #endregion
    }
}