using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.API.Controllers
{
   
    [Authorize(Roles = "Admin,Receptionist")] 
    public class MembershipController : BaseApiController
    {
        private readonly IMembershipRepo _membershipRepo;

        public MembershipController(IMembershipRepo membershipRepo)
        {
            _membershipRepo = membershipRepo ?? throw new ArgumentNullException(nameof(membershipRepo));
        }

      
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
                    new ApiExceptionResponse(500, "An error occurred while retrieving memberships", ex.Message));
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

      
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateMembership([FromBody] MonthlyMembershipDto membershipDto)
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
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while creating the membership", ex.Message));
            }
        }

      
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMembership(int id, [FromBody] MonthlyMembershipDto membershipDto)
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
                    new ApiExceptionResponse(500, $"An error occurred while deleting membership with ID {id}", ex.Message));
            }
        }

        /// <summary>
        /// Retrieves all active memberships (IsActive = true).
        /// </summary>
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
                    new ApiExceptionResponse(500, "An error occurred while retrieving active memberships", ex.Message));
            }
        }

        /// <summary>
        /// Retrieves all suspended memberships (IsActive = false).
        /// </summary>
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
                    new ApiExceptionResponse(500, "An error occurred while retrieving suspended memberships", ex.Message));
            }
        }

        /// <summary>
        /// Renews a membership by extending its end date and setting it to active.
        /// </summary>
        [HttpPost("renew/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RenewMembership(int id)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Membership ID must be a positive integer."));
            }

            try
            {
                var response = await _membershipRepo.RenewMembershipAsync(id);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while renewing membership with ID {id}", ex.Message));
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