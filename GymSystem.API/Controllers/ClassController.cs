using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GymSystem.API.Controllers
{
    //إضافة حصة 
    public class ClassController : BaseApiController
    {
        private readonly IClassRepo _classRepo;

      
        public ClassController(IClassRepo classRepo)
        {
            _classRepo = classRepo ?? throw new ArgumentNullException(nameof(classRepo));
        }

       
        [Authorize(Roles = "Admin,Receptionist,Trainer")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllClasses([FromQuery] SpecPrams specParams = null)
        {
            try
            {
                var classes = await _classRepo.GetClasses(specParams);
                var classList = classes.ToList();
                return Ok(new ApiResponse(200, "Classes retrieved successfully", classList));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving classes", ex.Message));
            }
        }

    
        [Authorize(Roles = "Admin,Receptionist,Trainer")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetClassById(int id)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Class ID must be a positive integer."));
            }

            try
            {
                var classDto = await _classRepo.GetClass(id);
                if (classDto == null)
                {
                    return NotFound(new ApiResponse(404, $"Class with ID {id} not found"));
                }

                return Ok(new ApiResponse(200, "Class retrieved successfully", classDto));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while retrieving class with ID {id}", ex.Message));
            }
        }

      
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateClass([FromBody] ClassDto classDto)
        {
            if (!IsValidModel(classDto))
            {
                return BadRequest(CreateValidationError("Invalid class data"));
            }

            try
            {
                var response = await _classRepo.AddClass(classDto);
                return HandleApiResponse(response, StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while creating the class", ex.Message));
            }
        }

      
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] ClassDto classDto)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Class ID must be a positive integer."));
            }

            if (!IsValidModel(classDto))
            {
                return BadRequest(CreateValidationError("Invalid class data"));
            }

            try
            {
                var response = await _classRepo.UpdateClass(id, classDto);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while updating class with ID {id}", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteClass(int id)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Class ID must be a positive integer."));
            }

            try
            {
                var response = await _classRepo.DeleteClass(id);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while deleting class with ID {id}", ex.Message));
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
                404 => NotFound(response),
                409 => Conflict(response),
                500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                _ => StatusCode(response.StatusCode ?? 500, response)
            };
        }

        #endregion
    }
}