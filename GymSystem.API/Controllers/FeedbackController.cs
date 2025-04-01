using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GymSystem.API.Controllers
{
    public class FeedbackController : BaseApiController
    {
        private readonly IFeedbackRepo _feedbackRepo;

        public FeedbackController(IFeedbackRepo feedbackRepo)
        {
            _feedbackRepo = feedbackRepo ?? throw new ArgumentNullException(nameof(feedbackRepo));
        }

        [Authorize(Roles = "Admin,Trainer")]
        [HttpGet("getFeedback")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFeedback([FromQuery] int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "Feedback ID must be a positive integer." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {
                var feedback = await _feedbackRepo.GetFeedbackByIdAsync(id);
                if (feedback == null)
                {
                    return NotFound(new ApiResponse(404, $"Feedback with ID {id} not found"));
                }

                return Ok(new ApiResponse(200, "Feedback retrieved successfully", feedback));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while retrieving feedback", ex.Message));
            }
        }

      
        [Authorize(Roles = "Admin,Trainer")]
        [HttpGet("getAllFeedbacks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllFeedbacks()
        {
            try
            {
                var feedbacks = await _feedbackRepo.GetAllFeedbacksAsync();
                return Ok(new ApiResponse(200, "Feedbacks retrieved successfully", feedbacks));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while retrieving feedbacks", ex.Message));
            }
        }

      
        [Authorize(Roles = "Member")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateFeedback([FromBody] FeedbackDto feedbackDto)
        {
            if (!ModelState.IsValid || feedbackDto == null)
            {
                return BadRequest(CreateValidationErrorResponse("Invalid feedback data"));
            }

            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
                {
                    return BadRequest(new ApiResponse(400, "UserId claim is missing or invalid in the token."));
                }

                feedbackDto.UserId = userIdClaim.Value;
                var response = await _feedbackRepo.CreateFeedbackAsync(feedbackDto);
                return response.StatusCode == 200
                    ? Ok(response)
                    : BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while creating feedback", ex.Message));
            }
        }

      
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "Feedback ID must be a positive integer." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {
                var response = await _feedbackRepo.DeleteFeedbackAsync(id);
                return response.StatusCode switch
                {
                    200 => Ok(response),
                    404 => NotFound(response),
                    _ => BadRequest(response)
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while deleting feedback", ex.Message));
            }
        }

       
        [Authorize(Roles = "Member")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateFeedback(int id, [FromBody] FeedbackDto feedbackDto)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "Feedback ID must be a positive integer." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            if (!ModelState.IsValid || feedbackDto == null)
            {
                return BadRequest(CreateValidationErrorResponse("Invalid feedback data"));
            }

            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
                {
                    return BadRequest(new ApiResponse(400, "UserId claim is missing or invalid in the token."));
                }

                var existingFeedback = await _feedbackRepo.GetFeedbackByIdAsync(id);
                if (existingFeedback == null)
                {
                    return NotFound(new ApiResponse(404, $"Feedback with ID {id} not found"));
                }

                if (existingFeedback.UserId != userIdClaim.Value)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(403, "You can only update your own feedback."));
                }

                var response = await _feedbackRepo.UpdateFeedbackAsync(id, feedbackDto);
                return response.StatusCode switch
                {
                    200 => Ok(response),
                    404 => NotFound(response),
                    _ => BadRequest(response)
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while updating feedback", ex.Message));
            }
        }

        #region Helper Methods
        private ApiValidationErrorResponse CreateValidationErrorResponse(string message)
        {
            return new ApiValidationErrorResponse
            {
                Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList(),
                StatusCode = 400,
                Message = message
            };
        }
        #endregion
    }
}