using GymSystem.BLL.Dtos;
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
     /// <summary>
     /// اضافة صنف
     /// </summary>
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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllEquipments([FromQuery] SpecPrams specParams)
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
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateEquipment([FromBody] EquipmentCreateDto equipmentCreateDto)
        {
            if (!IsValidModel(equipmentCreateDto))
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
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateEquipment(int id, [FromBody] EquipmentCreateDto equipmentCreateDto)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Equipment ID must be a positive integer."));
            }

            if (!IsValidModel(equipmentCreateDto))
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