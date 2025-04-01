using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.plan;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.PlanSpec;
using GymSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories
{
    public class PlanRepo : IPlanRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PlanRepo(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponse> GetPlans()
        {
            try
            {
                await CleanExpiredOffers();

                var plans = await _unitOfWork.Repository<Plan>().GetAllAsync();
                if (!plans.Any())
                {
                    return new ApiResponse(200, "No plans found.", new List<PlanViewDto>());
                }

                var planDtos = plans.Select(plan => _mapper.Map<PlanViewDto>(plan)).ToList();

                return new ApiResponse(200, "Plans retrieved successfully", planDtos);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, $"Failed to retrieve plans: {ex.Message}");
            }
        }

        public async Task<PlanViewDto> GetByIdAsync(int id)
        {
            try
            {
                var plan = await _unitOfWork.Repository<Plan>().GetByIdAsync(id);
                await CleanExpiredOffers(plan);

                return plan == null ? null : _mapper.Map<PlanViewDto>(plan);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve plan with ID {id} from the database.", ex);
            }
        }

        public async Task<ApiResponse> CreateAsync(PlanDto planDto)
        {
            try
            {
                var plan = _mapper.Map<Plan>(planDto);
                await _unitOfWork.Repository<Plan>().Add(plan);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to save the plan to the database.");
                }

                var createdDto = _mapper.Map<PlanDto>(plan);
                return new ApiResponse(201, "Plan created successfully", createdDto);
            }
            catch (DbUpdateException ex)
            {
                return new ApiExceptionResponse(400, "Failed to create plan due to database constraints.", ex.Message);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An error occurred while creating the plan", ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateAsync(int id, PlanDto planDto)
        {
            try
            {
                var existingPlan = await _unitOfWork.Repository<Plan>().GetByIdAsync(id);
                if (existingPlan == null)
                {
                    return new ApiResponse(404, $"Plan with ID {id} not found.");
                }

                _mapper.Map(planDto, existingPlan);
                _unitOfWork.Repository<Plan>().Update(existingPlan);

                var result = await _unitOfWork.Complete();
                var updatedDto = _mapper.Map<PlanDto>(existingPlan);
                return new ApiResponse(200, "Plan updated successfully", updatedDto);
            }
            catch (DbUpdateException ex)
            {
                return new ApiExceptionResponse(400, "Failed to update plan due to database constraints.", ex.Message);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An error occurred while updating the plan", ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            try
            {
                var plan = await _unitOfWork.Repository<Plan>().GetByIdAsync(id);
                if (plan == null)
                {
                    return new ApiResponse(404, $"Plan with ID {id} not found.");
                }

                _unitOfWork.Repository<Plan>().Delete(plan);
                await _unitOfWork.Complete();
                return new ApiResponse(200, "Plan deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An error occurred while deleting the plan", ex.Message);
            }
        }

        private async Task CleanExpiredOffers()
        {
            try
            {
                var plans = await _unitOfWork.Repository<Plan>().GetAllAsync();
                if (!plans.Any())
                {
                    return;
                }

                var offerSpec = new ActiveOffersSpecification();
                var activeOffers = await _unitOfWork.Repository<Offer>().GetAllWithSpecAsync(offerSpec);

                foreach (var plan in plans)
                {
                    if (plan.HasOffer && plan.ExpireDate.HasValue && plan.ExpireDate.Value.Date < DateTime.UtcNow.Date)
                    {
                        plan.HasOffer = false;
                        plan.DiscountedPrice = null;
                        plan.ExpireDate = null;
                        _unitOfWork.Repository<Plan>().Update(plan);

                        var relatedOffer = activeOffers.FirstOrDefault(o => o.PlanId == plan.Id);
                        if (relatedOffer != null)
                        {
                            relatedOffer.IsActive = false;
                            _unitOfWork.Repository<Offer>().Update(relatedOffer);
                        }
                    }
                }

                var result = await _unitOfWork.Complete();
                if (result < 0)
                {
                    throw new Exception("Failed to update plans and offers with expired offers.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to clean up expired offers.", ex);
            }
        }

        private async Task CleanExpiredOffers(Plan plan)
        {
            try
            {
                var fetchedPlan = await _unitOfWork.Repository<Plan>().GetByIdAsync(plan.Id);
                if (fetchedPlan == null)
                {
                    return;
                }

                var offerSpec = new ActiveOffersSpecification();
                var activeOffers = await _unitOfWork.Repository<Offer>().GetAllWithSpecAsync(offerSpec);

                if (fetchedPlan.HasOffer && fetchedPlan.ExpireDate.HasValue && fetchedPlan.ExpireDate.Value.Date < DateTime.UtcNow.Date)
                {
                    fetchedPlan.HasOffer = false;
                    fetchedPlan.DiscountedPrice = null;
                    fetchedPlan.ExpireDate = null;
                    _unitOfWork.Repository<Plan>().Update(fetchedPlan);

                    var relatedOffer = activeOffers.FirstOrDefault(o => o.PlanId == fetchedPlan.Id);
                    if (relatedOffer != null)
                    {
                        relatedOffer.IsActive = false;
                        _unitOfWork.Repository<Offer>().Update(relatedOffer);
                    }
                }

                var result = await _unitOfWork.Complete();
                if (result < 0)
                {
                    throw new Exception("Failed to update plans and offers with expired offers.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to clean up expired offers.", ex);
            }
        }
    }
}