using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.Equipment;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GymSystem.API.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    public class EquipmentController : BaseApiController
    {
        private readonly IEquipmentRepo _equipmentRepo;

        public EquipmentController(IEquipmentRepo equipmentRepo)
        {
            _equipmentRepo = equipmentRepo ?? throw new ArgumentNullException(nameof(equipmentRepo));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllEquipments([FromQuery] SpecPrams specParams = null)
        {
            try
            {
                var equipments = await _equipmentRepo.GetAllAsync(specParams);
                return Ok(new ApiResponse(200, "Equipments retrieved successfully", equipments));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving equipments", ex.Message));
            }
        }

       
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEquipmentById(int id)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Equipment ID must be a positive integer."));
            }

            try
            {
                var equipment = await _equipmentRepo.GetByIdAsync(id);
                if (equipment == null)
                {
                    return NotFound(new ApiResponse(404, $"Equipment with ID {id} not found"));
                }

                return Ok(new ApiResponse(200, "Equipment retrieved successfully", equipment));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while retrieving equipment with ID {id}", ex.Message));
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateEquipment([FromBody] EquipmentCreateDto equipmentCreateDto)
        {
            if (!ModelState.IsValid || equipmentCreateDto == null)
            {
                return BadRequest(CreateValidationError("Invalid equipment data"));
            }

            try
            {
                var response = await _equipmentRepo.CreateAsync(equipmentCreateDto);
                return HandleApiResponse(response, StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while creating the equipment", ex.Message));
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateEquipment(int id, [FromBody] EquipmentCreateDto equipmentCreateDto)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Equipment ID must be a positive integer."));
            }

            if (!ModelState.IsValid || equipmentCreateDto == null)
            {
                return BadRequest(CreateValidationError("Invalid equipment data"));
            }

            try
            {
                var response = await _equipmentRepo.UpdateAsync(id, equipmentCreateDto);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while updating equipment with ID {id}", ex.Message));
            }
        }

      
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteEquipment(int id)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Equipment ID must be a positive integer."));
            }

            try
            {
                var response = await _equipmentRepo.DeleteAsync(id);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while deleting equipment with ID {id}", ex.Message));
            }
        }

        [HttpPost("repair")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RepairEquipment([FromBody] EquipmentRepairDto repairDto)
        {
            if (!ModelState.IsValid || repairDto == null)
            {
                return BadRequest(CreateValidationError("Invalid repair data"));
            }

            try
            {
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new ApiResponse(401, "User not authenticated. Please provide a valid token."));
                }

                var response = await _equipmentRepo.RepairAsync(repairDto, currentUserId);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while repairing the equipment", ex.Message));
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