using AutoMapper;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Dtos.NutritionPlan;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.NutritionPlanSpec;
using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories.Business
{
    public class NutritionPlanRepo : INutritionPlanRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NutritionPlanRepo(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponse> CreateNutritionPlan(NutritionPlanDto nutritionPlanDto)
        {
            var spec = new NutritionPlanByNameSpecification(nutritionPlanDto.PlanName);

            var existingNutritionPlan = await _unitOfWork.Repository<NutritionPlan>().GetEntityWithSpecAsync(spec);

            if (existingNutritionPlan != null)
            {
                return new ApiResponse(400, "Nutrition Plan already exists");
            }

            try
            {
                var mappedNutritionPlan = _mapper.Map<NutritionPlan>(nutritionPlanDto);
                await _unitOfWork.Repository<NutritionPlan>().Add(mappedNutritionPlan);
                await _unitOfWork.Complete();
                return new ApiResponse(200, "Nutrition Plan added successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "Error: " + ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteNutritionPlan(int nutritionPlanId)
        {
            var spec = new NutritionPlanByIdSpecification(nutritionPlanId);

            var nutritionPlan = await _unitOfWork.Repository<NutritionPlan>().GetEntityWithSpecAsync(spec);

            if (nutritionPlan == null || nutritionPlan.IsDeleted)
            {
                return new ApiResponse(404, "Nutrition Plan not found");
            }

            try
            {
                nutritionPlan.IsDeleted = true;
                _unitOfWork.Repository<NutritionPlan>().Update(nutritionPlan);
                await _unitOfWork.Complete();

                return new ApiResponse(200, "Nutrition Plan deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "Error: " + ex.Message);
            }
        }

        public async Task<NutritionPlanDto> GetNutritionPlan(int nutritionPlanId)
        {
            var spec = new NutritionPlanByIdSpecification(nutritionPlanId);

            var nutritionPlan = await _unitOfWork.Repository<NutritionPlan>().GetEntityWithSpecAsync(spec);

            if (nutritionPlan == null || nutritionPlan.IsDeleted)
            {
                return null;
            }

            return _mapper.Map<NutritionPlanDto>(nutritionPlan);
        }

        public async Task<IEnumerable<NutritionPlanDto>> GetNutritionPlans()
        {
            var spec = new AllNutritionPlansSpecification();

            var nutritionPlans = await _unitOfWork.Repository<NutritionPlan>().GetAllWithSpecAsync(spec);

            return _mapper.Map<IEnumerable<NutritionPlanDto>>(nutritionPlans);
        }

        public async Task<ApiResponse> UpdateNutritionPlan(int nutritionPlanId, NutritionPlanDto nutritionPlanDto)
        {
            var spec = new NutritionPlanByIdWithoutDeletedCheckSpecification(nutritionPlanId);

            var nutritionPlan = await _unitOfWork.Repository<NutritionPlan>().GetEntityWithSpecAsync(spec);

            if (nutritionPlan == null)
            {
                return new ApiResponse(404, "Nutrition Plan not found");
            }

            try
            {
                nutritionPlan.PlanName = nutritionPlanDto.PlanName;
                nutritionPlan.Description = nutritionPlanDto.Description;

                _unitOfWork.Repository<NutritionPlan>().Update(nutritionPlan);

                await _unitOfWork.Complete();

                return new ApiResponse(200, "Nutrition Plan updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "Error: " + ex.Message);
            }
        }
    }
}