using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
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

        public async Task<IReadOnlyList<PlanDto>> GetAllAsync(SpecPrams specParams = null)
        {
            try
            {
                var spec = specParams != null ? new PlanWithFiltersSpecification(specParams) : null;
                var plans = await _unitOfWork.Repository<Plan>().GetAllWithSpecAsync(spec);
                var planDtos = plans.Select(p => _mapper.Map<PlanDto>(p)).ToList();
                return planDtos.AsReadOnly();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve plans from the database.", ex);
            }
        }

        public async Task<PlanDto> GetByIdAsync(int id)
        {
            try
            {
                var plan = await _unitOfWork.Repository<Plan>().GetByIdAsync(id);
                return plan == null ? null : _mapper.Map<PlanDto>(plan);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve plan with ID {id} from the database.", ex);
            }
        }

        public async Task<ApiResponse> CreateAsync(PlanDto planDto)
        {
            if (planDto == null)
            {
                return new ApiResponse(400, "Plan data cannot be null.");
            }

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
            if (id <= 0)
            {
                return new ApiResponse(400, "Plan ID must be a positive integer.");
            }

            if (planDto == null)
            {
                return new ApiResponse(400, "Plan data cannot be null.");
            }

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
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to update the plan in the database.");
                }

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
            if (id <= 0)
            {
                return new ApiResponse(400, "Plan ID must be a positive integer.");
            }

            try
            {
                var plan = await _unitOfWork.Repository<Plan>().GetByIdAsync(id);
                if (plan == null)
                {
                    return new ApiResponse(404, $"Plan with ID {id} not found.");
                }

                _unitOfWork.Repository<Plan>().Delete(plan);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to delete the plan from the database.");
                }

                return new ApiResponse(200, "Plan deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An error occurred while deleting the plan", ex.Message);
            }
        }
    }

}