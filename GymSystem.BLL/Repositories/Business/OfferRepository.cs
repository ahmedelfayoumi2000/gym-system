using AutoMapper;
using GymSystem.BLL.Dtos.Offer;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.OfferSpec;
using GymSystem.DAL.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories.Business
{
    public class OfferRepository : IOfferRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<OfferRepository> _logger;

        public OfferRepository(IUnitOfWork unitOfWork, IMapper mapper, ILogger<OfferRepository> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse> AddOffer(OfferDto offerDto)
        {
            if (offerDto == null)
            {
                _logger.LogWarning("AddOffer operation aborted: OfferDto is null.");
                return new ApiResponse(400, "Offer data cannot be null.");
            }

            _logger.LogInformation("Attempting to add offer for Plan ID: {PlanId}", offerDto.PlanId);

            try
            {
                var plan = await _unitOfWork.Repository<Plan>().GetByIdAsync(offerDto.PlanId);
                if (plan == null)
                {
                    _logger.LogWarning("Plan with ID {PlanId} not found.", offerDto.PlanId);
                    return new ApiResponse(404, $"Plan with ID {offerDto.PlanId} not found.");
                }

                // التأكد من إن Price مش null
                if (plan.Price == null)
                {
                    _logger.LogWarning("Plan with ID {PlanId} has a null Price.", offerDto.PlanId);
                    return new ApiResponse(400, "Plan price cannot be null.");
                }

                if (offerDto.DiscountedPrice >= plan.Price)
                {
                    _logger.LogWarning("Discounted price {DiscountedPrice} is not less than original price {OriginalPrice} for Plan ID {PlanId}.",
                        offerDto.DiscountedPrice, plan.Price, offerDto.PlanId);
                    return new ApiResponse(400, "Discounted price must be less than the original price.");
                }

                if (!IsValidDateRange(offerDto.StartDate, offerDto.EndDate))
                {
                    _logger.LogWarning("Invalid date range for offer on Plan ID {PlanId}. Start: {StartDate}, End: {EndDate}",
                        offerDto.PlanId, offerDto.StartDate, offerDto.EndDate);
                    return new ApiResponse(400, "Start date must be before end date and not in the past.");
                }

                // التحقق من ان فيه عرض ساري على نفس الخطة
                var activeOfferSpec = new ActiveOfferByPlanIdSpecification(offerDto.PlanId);
                var existingOffer = await _unitOfWork.Repository<Offer>().GetEntityWithSpecAsync(activeOfferSpec);
                if (existingOffer != null)
                {
                    _logger.LogWarning("An active offer already exists for Plan ID {PlanId}. Existing offer ID: {OfferId}, valid until: {EndDate}.",
                        offerDto.PlanId, existingOffer.Id, existingOffer.EndDate);
                    return new ApiResponse(409, $"لا يمكن إضافة عرض جديد، هناك عرض ساري بالفعل على هذه الخطة حتى تاريخ {existingOffer.EndDate:yyyy-MM-dd}.");
                }

                var offerEntity = _mapper.Map<Offer>(offerDto);
                await _unitOfWork.Repository<Offer>().Add(offerEntity);

                // تحديث الـ Plan
                plan.HasOffer = true;
                plan.DiscountedPrice = offerEntity.DiscountedPrice;
                plan.ExpireDate = offerEntity.EndDate;

                _unitOfWork.Repository<Plan>().Update(plan);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("AddOffer failed: No rows affected for Plan ID {PlanId}.", offerDto.PlanId);
                    return new ApiExceptionResponse(500, "Failed to add offer due to database error.");
                }

                var createdDto = _mapper.Map<OfferViewDto>(offerEntity);
                createdDto.PlanName = plan.PlanName ?? "Unknown Plan";
                createdDto.OriginalPrice = plan.Price;
                _logger.LogInformation("Offer for Plan ID {PlanId} added successfully with ID {Id}.", offerDto.PlanId, offerEntity.Id);
                return new ApiResponse(201, "Offer added successfully", createdDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding offer for Plan ID: {PlanId}", offerDto.PlanId);
                return new ApiExceptionResponse(500, $"Failed to add offer: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateOffer(int id, UpdateOffer offerDto)
        {
            if (offerDto == null)
            {
                _logger.LogWarning("UpdateOffer operation aborted for ID {Id}: OfferDto is null.", id);
                return new ApiResponse(400, "Offer data cannot be null.");
            }

            _logger.LogInformation("Attempting to update offer with ID: {Id}", id);

            try
            {
                var existingOffer = await _unitOfWork.Repository<Offer>().GetByIdAsync(id);
                if (existingOffer == null /*|| !existingOffer.IsActive*/)
                {
                    _logger.LogWarning("Offer with ID {Id} not found or already deleted.", id);
                    return new ApiResponse(404, $"Offer with ID {id} not found or already deleted.");
                }

                var plan = await _unitOfWork.Repository<Plan>().GetByIdAsync((int)existingOffer.PlanId);
                if (plan == null)
                {
                    _logger.LogWarning("Plan with ID {PlanId} not found for offer ID {Id}.", existingOffer.PlanId, id);
                    return new ApiResponse(404, $"Plan with ID {existingOffer.PlanId} not found.");
                }

                if (offerDto.DiscountedPrice >= plan.Price)
                {
                    _logger.LogWarning("Discounted price {DiscountedPrice} is not less than original price {OriginalPrice} for Plan ID {PlanId}.",
                        offerDto.DiscountedPrice, plan.Price, existingOffer.PlanId);
                    return new ApiResponse(400, "Discounted price must be less than the original price.");
                }

                if (!IsValidDateRange(offerDto.StartDate, offerDto.EndDate))
                {
                    _logger.LogWarning("Invalid date range for offer on Plan ID {PlanId}. Start: {StartDate}, End: {EndDate}",
                        existingOffer.PlanId, offerDto.StartDate, offerDto.EndDate);
                    return new ApiResponse(400, "Start date must be before end date and not in the past.");
                }

                _mapper.Map(offerDto, existingOffer);

                _unitOfWork.Repository<Offer>().Update(existingOffer);

                plan.HasOffer = true;
                plan.DiscountedPrice = existingOffer.DiscountedPrice;
                plan.ExpireDate = existingOffer.EndDate;
                _unitOfWork.Repository<Plan>().Update(plan);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("UpdateOffer failed: No rows affected for ID {Id}.", id);
                    return new ApiExceptionResponse(500, "Failed to update offer due to database error.");
                }

                var updatedDto = _mapper.Map<OfferViewDto>(existingOffer);
                updatedDto.PlanName = plan.PlanName;
                updatedDto.OriginalPrice = plan.Price;
                _logger.LogInformation("Offer with ID {Id} updated successfully.", id);
                return new ApiResponse(200, "Offer updated successfully", updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating offer with ID: {Id}", id);
                return new ApiExceptionResponse(500, $"Failed to update offer: {ex.Message}");
            }
        }

        public async Task<ApiResponse> DeleteOffer(int id)
        {
            _logger.LogInformation("Attempting to delete offer with ID: {Id}", id);

            try
            {
                var offerEntity = await _unitOfWork.Repository<Offer>().GetByIdAsync(id);
                if (offerEntity == null

 || !offerEntity.IsActive)
                {
                    _logger.LogWarning("Offer with ID {Id} not found or already deleted.", id);
                    return new ApiResponse(404, $"Offer with ID {id} not found or already deleted.");
                }
                var plan = await _unitOfWork.Repository<Plan>().GetByIdAsync((int)offerEntity.PlanId);

                plan.HasOffer = false;
                plan.DiscountedPrice = null;
                plan.ExpireDate = null;

                _unitOfWork.Repository<Offer>().Delete(offerEntity);
                _unitOfWork.Repository<Plan>().Update(plan);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("DeleteOffer failed: No rows affected for ID {Id}.", id);
                    return new ApiExceptionResponse(500, "Failed to delete offer due to database error.");
                }

                _logger.LogInformation("Offer with ID {Id} deleted successfully.", id);
                return new ApiResponse(200, "Offer deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting offer with ID: {Id}", id);
                return new ApiExceptionResponse(500, $"Failed to delete offer: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetOffer(int id)
        {
            _logger.LogInformation("Retrieving offer with ID: {Id}", id);

            try
            {
                var spec = new OfferByIdSpecification(id);
                var offerEntity = await _unitOfWork.Repository<Offer>().GetEntityWithSpecAsync(spec);
                if (offerEntity == null)
                {
                    _logger.LogWarning("Offer with ID {Id} not found.", id);
                    return new ApiResponse(404, $"Offer with ID {id} not found.");
                }

                var offerDto = _mapper.Map<OfferViewDto>(offerEntity);
                offerDto.PlanName = offerEntity.Plan.PlanName;
                offerDto.OriginalPrice = offerEntity.Plan.Price;
                _logger.LogInformation("Offer with ID {Id} retrieved successfully.", id);
                return new ApiResponse(200, "Offer retrieved successfully", offerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving offer with ID: {Id}", id);
                return new ApiExceptionResponse(500, $"Failed to retrieve offer: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetOffers()
        {
            _logger.LogInformation("Retrieving all active offers.");

            try
            {
                var spec = new AllActiveOffersSpecification();
                var offers = await _unitOfWork.Repository<Offer>().GetAllWithSpecAsync(spec);
                var offerDtos = _mapper.Map<IEnumerable<OfferViewDto>>(offers);

                foreach (var offerDto in offerDtos)
                {
                    var plan = offers.First(o => o.Id == offerDto.Id).Plan;
                    offerDto.PlanName = plan.PlanName;
                    offerDto.OriginalPrice = plan.Price;
                }

                if (!offerDtos.Any())
                {
                    _logger.LogInformation("No active offers found.");
                    return new ApiResponse(200, "No offers found.", new List<OfferViewDto>());
                }

                _logger.LogInformation("Retrieved {Count} active offers.", offerDtos.Count());
                return new ApiResponse(200, "Offers retrieved successfully", offerDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all offers.");
                return new ApiExceptionResponse(500, $"Failed to retrieve offers: {ex.Message}");
            }
        }

        private bool IsValidDateRange(DateTime startDate, DateTime endDate)
        {
            var startDateOnly = startDate.Date;
            var endDateOnly = endDate.Date;
            var currentDate = DateTime.UtcNow.Date;

            return startDateOnly < endDateOnly && startDateOnly >= currentDate;
        }
    }
}