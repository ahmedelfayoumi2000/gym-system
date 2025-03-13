using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Interfaces;
using GymSystem.DAL.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories.Business
{
    public class FeedbackRepo : IFeedbackRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public FeedbackRepo(
            IUnitOfWork unitOfWork,
            IMapper mapper
            )
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponse> CreateFeedbackAsync(FeedbackDto feedbackDto)
        {
            try
            {
                var feedback = _mapper.Map<Feedback>(feedbackDto);

                var feedbackRepo = _unitOfWork.Repository<Feedback>();
                await feedbackRepo.Add(feedback);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to save feedback");
                }

                return new ApiResponse(200, "Feedback added successfully", _mapper.Map<FeedbackDto>(feedback));
            }
            catch (Exception ex)
            {
                return new ApiResponse(400, $"Error adding feedback: {ex.Message}");
            }
        }

        public async Task<ApiResponse> DeleteFeedbackAsync(int id)
        {
            try
            {
                var feedbackRepo = _unitOfWork.Repository<Feedback>();
                var feedback = await feedbackRepo.GetByIdAsync(id);

                if (feedback == null)
                {
                    return new ApiResponse(404, "Feedback not found");
                }

                feedbackRepo.Delete(feedback);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to delete feedback");
                }

                return new ApiResponse(200, "Feedback deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(400, $"Error deleting feedback: {ex.Message}");
            }
        }

        public async Task<IReadOnlyList<FeedbackDto>> GetAllFeedbacksAsync()
        {
            try
            {
                var feedbackRepo = _unitOfWork.Repository<Feedback>();
                var feedbacks = await feedbackRepo.GetAllAsync();

                var feedbackDtos = _mapper.Map<IReadOnlyList<FeedbackDto>>(feedbacks);
                return feedbackDtos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving feedbacks: {ex.Message}");
            }
        }

        public async Task<FeedbackDto> GetFeedbackByIdAsync(int id)
        {
            try
            {
                var feedbackRepo = _unitOfWork.Repository<Feedback>();
                var feedback = await feedbackRepo.GetByIdAsync(id);

                if (feedback == null)
                {
                    return null;
                }

                var feedbackDto = _mapper.Map<FeedbackDto>(feedback);
                return feedbackDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving feedback: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateFeedbackAsync(int id, FeedbackDto feedbackDto)
        {
            try
            {
                var feedbackRepo = _unitOfWork.Repository<Feedback>();
                var existingFeedback = await feedbackRepo.GetByIdAsync(id);

                if (existingFeedback == null)
                {
                    return new ApiResponse(404, "Feedback not found");
                }

                existingFeedback.Comments = feedbackDto.Comments;
                existingFeedback.Rating = feedbackDto.Rating;

                feedbackRepo.Update(existingFeedback);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to update feedback");
                }

                return new ApiResponse(200, "Feedback updated successfully", _mapper.Map<FeedbackDto>(existingFeedback));
            }
            catch (Exception ex)
            {
                return new ApiResponse(400, $"Error updating feedback: {ex.Message}");
            }
        }
    }
}
