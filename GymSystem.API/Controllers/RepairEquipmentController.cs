using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymSystem.API.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    
    public class RepairEquipmentController : BaseApiController
    {
        private readonly IRepairEquipmentRepo _repairEquipmentRepo;

        public RepairEquipmentController(IRepairEquipmentRepo repairEquipmentRepo)
        {
            _repairEquipmentRepo = repairEquipmentRepo ?? throw new ArgumentNullException(nameof(repairEquipmentRepo));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRepairs()
        {
            try
            {
                var repairs = await _repairEquipmentRepo.GetAllAsync();
                return Ok(new ApiResponse(200, "Repairs retrieved successfully", repairs));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving repairs.", ex.Message));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRepairsForEquipmentId(int id)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Equipment ID must be a positive integer."));
            }

            try
            {
                var repairs = await _repairEquipmentRepo.GetRepairsByEquipmentIdAsync(id);
                if (repairs == null || !repairs.Any())
                {
                    return NotFound(new ApiResponse(404, $"No repairs found for Equipment with ID {id}."));
                }

                return Ok(new ApiResponse(200, "Repair details for Equipment retrieved successfully", repairs));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while retrieving repairs for Equipment with ID {id}.", ex.Message));
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddRepair([FromBody] RepairDto repairDto)
        {
            if (!IsValidModel(repairDto))
            {
                return BadRequest(CreateValidationError("Invalid repair data."));
            }

            try
            {
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new ApiResponse(401, "User not authenticated. Please provide a valid token."));
                }

                var response = await _repairEquipmentRepo.CreateAsync(repairDto, currentUserId);
                return HandleApiResponse(response, StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while adding a repair for equipment.", ex.Message));
            }
        }

        #region Private Helper Methods

        private bool IsValidId(int id) => id > 0;

        private bool IsValidModel(object model) => ModelState.IsValid && model != null;

        private ApiValidationErrorResponse CreateValidationError(string message)
        {
            return new ApiValidationErrorResponse
            {
                Errors = new List<string> { message },
                StatusCode = 400,
                Message = "Invalid request data"
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
                404 => NotFound(response),
                500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                _ => StatusCode(response.StatusCode ?? 500, response)
            };
        }

        #endregion
    }
}