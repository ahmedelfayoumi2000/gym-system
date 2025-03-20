using GymSystem.BLL.Dtos.Offer;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GymSystem.API.Controllers
{
    public class OfferController : BaseApiController
    {
        private readonly IOfferRepo _offerRepo;

        public OfferController(IOfferRepo offerRepo)
        {
            _offerRepo = offerRepo ?? throw new ArgumentNullException(nameof(offerRepo));
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateOffer([FromBody] OfferDto offerDto)
        {
            if (offerDto == null || !ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ApiValidationErrorResponse
                {
                    StatusCode = 400,
                    Message = "Invalid offer data.",
                    Errors = errors
                });
            }

            try
            {
                var response = await _offerRepo.AddOffer(offerDto);
                return HandleApiResponse(response, StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while creating the offer.", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Receptionist,Trainer")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllOffers()
        {
            try
            {
                var response = await _offerRepo.GetOffers();
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving offers.", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Receptionist,Trainer")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOfferById(int id)
        {
            if (!IsValidId(id))
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    StatusCode = 400,
                    Message = "Invalid request data.",
                    Errors = new[] { "Offer ID must be a positive integer." }
                });
            }

            try
            {
                var response = await _offerRepo.GetOffer(id);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while retrieving offer with ID {id}.", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateOffer(int id, [FromBody] OfferDto offerDto)
        {
            if (!IsValidId(id))
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    StatusCode = 400,
                    Message = "Invalid request data.",
                    Errors = new[] { "Offer ID must be a positive integer." }
                });
            }

            if (offerDto == null || !ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ApiValidationErrorResponse
                {
                    StatusCode = 400,
                    Message = "Invalid offer data.",
                    Errors = errors
                });
            }

            try
            {
                var response = await _offerRepo.UpdateOffer(id, offerDto);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while updating offer with ID {id}.", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteOffer(int id)
        {
            if (!IsValidId(id))
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    StatusCode = 400,
                    Message = "Invalid request data.",
                    Errors = new[] { "Offer ID must be a positive integer." }
                });
            }

            try
            {
                var response = await _offerRepo.DeleteOffer(id);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while deleting offer with ID {id}.", ex.Message));
            }
        }

        private bool IsValidId(int id) => id > 0;

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
    }
}