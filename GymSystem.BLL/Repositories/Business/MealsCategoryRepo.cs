using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.MealsCategorySpec;
using GymSystem.DAL.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories.Business
{
    public class MealsCategoryRepository : IMealsCategoryRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MealsCategoryRepository> _logger;

        public MealsCategoryRepository(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<MealsCategoryRepository> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse> Add(MealsCategoryDto mealsCategory)
        {
            if (mealsCategory == null)
            {
                _logger.LogWarning("Attempted to add a null MealsCategoryDto.");
                return new ApiResponse(400, "Meals category data cannot be null.");
            }

            try
            {
                _logger.LogInformation("Attempting to add meals category with name: {CategoryName}", mealsCategory.CategoryName);

                var spec = new MealsCategoryByNameSpecification(mealsCategory.CategoryName);
                var existingCategory = await _unitOfWork.Repository<MealsCategory>().GetEntityWithSpecAsync(spec);
                if (existingCategory != null)
                {
                    _logger.LogWarning("Meals category with name {CategoryName} already exists.", mealsCategory.CategoryName);
                    return new ApiResponse(409, $"Meals category '{mealsCategory.CategoryName}' already exists.");
                }

                var categoryEntity = _mapper.Map<MealsCategory>(mealsCategory);
                await _unitOfWork.Repository<MealsCategory>().Add(categoryEntity);
                await _unitOfWork.Complete();

                _logger.LogInformation("Meals category {CategoryName} added successfully with ID: {Id}", mealsCategory.CategoryName, categoryEntity.Id);
                return new ApiResponse(201, "Meals category added successfully", _mapper.Map<MealsCategoryDto>(categoryEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding meals category with name: {CategoryName}", mealsCategory.CategoryName);
                return new ApiResponse(500, $"Failed to add meals category: {ex.Message}");
            }
        }

        public async Task<ApiResponse> Delete(int mealsCategoryId)
        {
            try
            {
                _logger.LogInformation("Attempting to delete meals category with ID: {Id}", mealsCategoryId);

                var spec = new MealsCategoryByIdSpecification(mealsCategoryId);
                var category = await _unitOfWork.Repository<MealsCategory>().GetEntityWithSpecAsync(spec);
                if (category == null)
                {
                    _logger.LogWarning("Meals category with ID {Id} not found or already deleted.", mealsCategoryId);
                    return new ApiResponse(404, $"Meals category with ID {mealsCategoryId} not found.");
                }

                category.IsDeleted = true;
                _unitOfWork.Repository<MealsCategory>().Update(category);
                await _unitOfWork.Complete();

                _logger.LogInformation("Meals category with ID {Id} deleted successfully.", mealsCategoryId);
                return new ApiResponse(200, "Meals category deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting meals category with ID: {Id}", mealsCategoryId);
                return new ApiResponse(500, $"Failed to delete meals category: {ex.Message}");
            }
        }

        public async Task<IEnumerable<MealsCategoryDto>> GetAllMealsCategory()
        {
            try
            {
                _logger.LogInformation("Retrieving all active meals categories.");

                var spec = new AllMealsCategoriesSpecification();
                var categories = await _unitOfWork.Repository<MealsCategory>().GetAllWithSpecAsync(spec);
                var categoryDtos = _mapper.Map<IEnumerable<MealsCategoryDto>>(categories);

                _logger.LogInformation("Retrieved {Count} active meals categories.", categoryDtos.Count());
                return categoryDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all meals categories.");
                throw new ApplicationException($"Failed to retrieve meals categories: {ex.Message}", ex);
            }
        }

        public async Task<MealsCategoryDto> GetMealsCategoryById(int mealsCategoryId)
        {
            try
            {
                _logger.LogInformation("Retrieving meals category with ID: {Id}", mealsCategoryId);

                var spec = new MealsCategoryByIdSpecification(mealsCategoryId);
                var category = await _unitOfWork.Repository<MealsCategory>().GetEntityWithSpecAsync(spec);
                if (category == null)
                {
                    _logger.LogWarning("Meals category with ID {Id} not found.", mealsCategoryId);
                    return null;
                }

                var categoryDto = _mapper.Map<MealsCategoryDto>(category);
                _logger.LogInformation("Meals category with ID {Id} retrieved successfully.", mealsCategoryId);
                return categoryDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving meals category with ID: {Id}", mealsCategoryId);
                throw new ApplicationException($"Failed to retrieve meals category: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse> Update(MealsCategoryDto mealsCategory)
        {
            if (mealsCategory == null)
            {
                _logger.LogWarning("Attempted to update meals category with null MealsCategoryDto.");
                return new ApiResponse(400, "Meals category data cannot be null.");
            }

            try
            {
                _logger.LogInformation("Attempting to update meals category with ID: {Id}", mealsCategory.MealsCategoryId);

                var spec = new MealsCategoryByIdSpecification(mealsCategory.MealsCategoryId);
                var existingCategory = await _unitOfWork.Repository<MealsCategory>().GetEntityWithSpecAsync(spec);
                if (existingCategory == null)
                {
                    _logger.LogWarning("Meals category with ID {Id} not found or already deleted.", mealsCategory.MealsCategoryId);
                    return new ApiResponse(404, $"Meals category with ID {mealsCategory.MealsCategoryId} not found.");
                }

                _mapper.Map(mealsCategory, existingCategory); // Map DTO to existing entity
                _unitOfWork.Repository<MealsCategory>().Update(existingCategory);
                await _unitOfWork.Complete();

                _logger.LogInformation("Meals category with ID {Id} updated successfully.", mealsCategory.MealsCategoryId);
                return new ApiResponse(200, "Meals category updated successfully", _mapper.Map<MealsCategoryDto>(existingCategory));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating meals category with ID: {Id}", mealsCategory.MealsCategoryId);
                return new ApiResponse(500, $"Failed to update meals category: {ex.Message}");
            }
        }
    }
}