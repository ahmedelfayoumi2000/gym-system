using AutoMapper;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Dtos.NutritionPlan;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories.Business
{
    public class NutritionPlanRepo : INutritionPlanRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NutritionPlanRepo(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse> CreateNutritionPlan(NutritionPlanDto nutritionPlanDto)
        {
            // Build a specification to check if the Nutrition Plan already exists
            var spec = new BaseSpecification<NutritionPlan>(x => x.PlanName == nutritionPlanDto.PlanName && !x.IsDeleted);

            // Use the specification to get the Nutrition Plan
            var existingNutritionPlan = await _unitOfWork.Repository<NutritionPlan>().GetByIdWithSpecAsync(spec);

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
            // Build a specification to find the Nutrition Plan by ID and ensure it's not deleted
            var spec = new BaseSpecification<NutritionPlan>(x => x.Id == nutritionPlanId && !x.IsDeleted);

            // Use the specification to get the Nutrition Plan
            var nutritionPlan = await _unitOfWork.Repository<NutritionPlan>().GetByIdWithSpecAsync(spec);

            if (nutritionPlan == null || nutritionPlan.IsDeleted)
            {
                return new ApiResponse(404, "Nutrition Plan not found");
            }

            try
            {
                // Mark the Nutrition Plan as deleted
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
            // Build a specification to find the Nutrition Plan by ID and ensure it's not deleted
            var spec = new BaseSpecification<NutritionPlan>(x => x.Id == nutritionPlanId && !x.IsDeleted);

            // Use the specification to get the Nutrition Plan
            var nutritionPlan = await _unitOfWork.Repository<NutritionPlan>().GetByIdWithSpecAsync(spec);

            if (nutritionPlan == null || nutritionPlan.IsDeleted)
            {
                return null;
            }

            // Map Entity to DTO and return
            return _mapper.Map<NutritionPlanDto>(nutritionPlan);
        }

        public async Task<IEnumerable<NutritionPlanDto>> GetNutritionPlans()
        {
            var spec = new BaseSpecification<NutritionPlan>(x => !x.IsDeleted);

            var nutritionPlans = await _unitOfWork.Repository<NutritionPlan>().GetAllWithSpecAsync(spec);

            return _mapper.Map<IEnumerable<NutritionPlanDto>>(nutritionPlans);
        }

        public async Task<ApiResponse> UpdateNutritionPlan(int nutritionPlanId, NutritionPlanDto nutritionPlanDto)
        {
            var spec = new BaseSpecification<NutritionPlan>(x => x.Id == nutritionPlanId);

            var nutritionPlan = await _unitOfWork.Repository<NutritionPlan>().GetByIdWithSpecAsync(spec);

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
