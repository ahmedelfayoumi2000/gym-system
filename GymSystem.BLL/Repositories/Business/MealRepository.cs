using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.MealSpec;
using GymSystem.DAL.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories.Business
{
    public class MealRepository : IMealRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MealRepository> _logger;

        public MealRepository(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<MealRepository> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse> CreateMeal(MealDto meal)
        {
            if (meal == null)
            {
                return new ApiResponse(400, "Meal data cannot be null.");
            }

            try
            {

                var spec = new MealByNameSpecification(meal.MealName);
                var existingMeal = await _unitOfWork.Repository<Meal>().GetEntityWithSpecAsync(spec);
                if (existingMeal != null)
                {
                    _logger.LogWarning("Meal with name {MealName} already exists.", meal.MealName);
                    return new ApiResponse(409, $"Meal '{meal.MealName}' already exists.");
                }

                var mealEntity = _mapper.Map<Meal>(meal);
                await _unitOfWork.Repository<Meal>().Add(mealEntity);
                await _unitOfWork.Complete();

                return new ApiResponse(201, "Meal added successfully", _mapper.Map<MealDto>(mealEntity));
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, $"Failed to add meal: {ex.Message}");
            }
        }

        public async Task<ApiResponse> DeleteMeal(int id)
        {
            try
            {

                var spec = new MealByIdSpecification(id);
                var meal = await _unitOfWork.Repository<Meal>().GetEntityWithSpecAsync(spec);
                if (meal == null)
                {
                    return new ApiResponse(404, $"Meal with ID {id} not found.");
                }

                meal.IsDeleted = true;
                _unitOfWork.Repository<Meal>().Update(meal);
                await _unitOfWork.Complete();

                return new ApiResponse(200, "Meal deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, $"Failed to delete meal: {ex.Message}");
            }
        }

        public async Task<IEnumerable<MealDto>> GetAllMeals()
        {
            try
            {

                var spec = new AllMealsSpecification();
                var meals = await _unitOfWork.Repository<Meal>().GetAllWithSpecAsync(spec);
                var mealDtos = _mapper.Map<IEnumerable<MealDto>>(meals);

                return mealDtos;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve meals: {ex.Message}", ex);
            }
        }

        public async Task<MealDto> GetMealById(int id)
        {
            try
            {
                var spec = new MealByIdSpecification(id);
                var meal = await _unitOfWork.Repository<Meal>().GetEntityWithSpecAsync(spec);
                if (meal == null)
                {
                    return null;
                }

                var mealDto = _mapper.Map<MealDto>(meal);
                return mealDto;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve meal: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse> UpdateMeal(int id, MealDto meal)
        {
            if (meal == null)
            {
                return new ApiResponse(400, "Meal data cannot be null.");
            }

            try
            {

                var spec = new MealByIdSpecification(id);
                var mealToUpdate = await _unitOfWork.Repository<Meal>().GetEntityWithSpecAsync(spec);
                if (mealToUpdate == null)
                {
                    return new ApiResponse(404, $"Meal with ID {id} not found.");
                }

                _mapper.Map(meal, mealToUpdate); 
                _unitOfWork.Repository<Meal>().Update(mealToUpdate);
                await _unitOfWork.Complete();

                return new ApiResponse(200, "Meal updated successfully", _mapper.Map<MealDto>(mealToUpdate));
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, $"Failed to update meal: {ex.Message}");
            }
        }
    }
}