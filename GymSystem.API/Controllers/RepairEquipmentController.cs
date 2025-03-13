using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Repositories;
using GymSystem.BLL.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.API.Controllers
{


    [Authorize(Roles = "Admin,Receptionist")]
    public class RepairEquipmentController : BaseApiController
    {
        private readonly IRepairEquipmentRepo _repairEquipmentRepo;

        public RepairEquipmentController(IRepairEquipmentRepo repairEquipmentRepo)
        {
            _repairEquipmentRepo = repairEquipmentRepo;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRepairs()
        {
            try
            {
                var Repairs = await _repairEquipmentRepo.GetAllAsync();
                return Ok(new ApiResponse(200, "Repairs retrieved successfully", Repairs));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving Repairs", ex.Message));
            }
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRepairforEquipmentId(int id)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Equipment ID must be a positive integer."));
            }

            try
            {
                var Repair = await _repairEquipmentRepo.GetRepairsByEquipmentIdAsync(id);
                if (Repair == null)
                {
                    return NotFound(new ApiResponse(404, $"Equipment with ID {id} not found"));
                }

                return Ok(new ApiResponse(200, "Repair Details for Equipment retrieved successfully", Repair));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while retrieving Repairs for equipment with ID {id}", ex.Message));
            }
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddRepair([FromBody] RepairDto RepairDto)
        {
            if (!IsValidModel(RepairDto))
            {
                return BadRequest(CreateValidationError("Invalid Repar data"));
            }

            try
            {
                var response = await _repairEquipmentRepo.CreateAsync(RepairDto);
                return HandleApiResponse(response, StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while Adding a Repair for equipment", ex.Message));
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
                404 => NotFound(response),
                400 => BadRequest(response),
                500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                _ => StatusCode(response.StatusCode ?? 500, response)
            };
        }

        #endregion
    }
}
