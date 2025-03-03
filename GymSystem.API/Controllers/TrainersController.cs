using GymSystem.API.DTOs.Trainer;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Dtos.Trainer;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymSystem.API.Controllers
{
   
    [Authorize(Roles = "Admin")] 
    public class TrainersController : BaseApiController
    {
        private readonly ITrainerService _trainerService;

        public TrainersController(ITrainerService trainerService)
        {
            _trainerService = trainerService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse>> GetTrainerById(string id)
        {
            var trainer = await _trainerService.GetTrainerByIdAsync(id);
            if (trainer == null)
                return new ApiResponse(404, "Trainer not found");

            return new ApiResponse(200, "Success", trainer);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetAllTrainers()
        {
            var trainers = await _trainerService.GetAllTrainersAsync();
            return new ApiResponse(200, "Success", trainers);
        }


        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateTrainer([FromBody] CreateTrainerDto trainerDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                return new ApiValidationErrorResponse { Errors = errors };
            }

            try
            {
                var createdTrainer = await _trainerService.CreateTrainerAsync(trainerDto);
                return new ApiResponse(201, "Trainer created successfully", createdTrainer);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An error occurred while creating the trainer", ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> UpdateTrainer(string id, [FromBody] UpdateTrainerDto trainerDto)
        {
            if (!ModelState.IsValid)
                return new ApiValidationErrorResponse { Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) };

            var success = await _trainerService.UpdateTrainerAsync(id, trainerDto);
            if (!success)
                return new ApiResponse(404, "Trainer not found or update failed");

            return new ApiResponse(200, "Trainer updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteTrainer(string id)
        {
            var success = await _trainerService.DeleteTrainerAsync(id);
            if (!success)
                return new ApiResponse(404, "Trainer not found or deletion failed");

            return new ApiResponse(204, "Trainer deleted successfully");
        }

        [HttpPost("{id}/suspend")]
        public async Task<IActionResult> SuspendTrainer(string id)
        {
            var result = await _trainerService.SuspendTrainerAsync(id);

            if (!result)
            {
                return BadRequest(new ApiResponse(400, "Failed to suspend trainer."));
            }

            return Ok(new ApiResponse(200, "Trainer suspended successfully."));
        }
    }
}