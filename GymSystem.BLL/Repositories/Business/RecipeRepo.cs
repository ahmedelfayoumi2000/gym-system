using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.RecipeSpec;
using GymSystem.DAL.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories.Business
{
    public class RecipeRepo : IRecipeRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RecipeRepo> _logger;

        public RecipeRepo(IUnitOfWork unitOfWork, IMapper mapper, ILogger<RecipeRepo> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<RecipeDto>> GetRecipesAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all active recipes.");

                var spec = new AllRecipesSpecification();
                var recipes = await _unitOfWork.Repository<Recipe>().GetAllWithSpecAsync(spec);
                var recipeDtos = _mapper.Map<IEnumerable<RecipeDto>>(recipes);

                return recipeDtos;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve recipes: {ex.Message}", ex);
            }
        }

        public async Task<RecipeDto> GetRecipeAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            try
            {

                var spec = new RecipeByIdSpecification(id);
                var recipe = await _unitOfWork.Repository<Recipe>().GetEntityWithSpecAsync(spec);
                if (recipe == null)
                {
                    return null;
                }

                var recipeDto = _mapper.Map<RecipeDto>(recipe);
                return recipeDto;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve recipe: {ex.Message}", ex);
            }
        }

        public async Task<RecipeDto> AddRecipeAsync(RecipeDto recipeDto)
        {
            try
            {

                var mealCategory = await _unitOfWork.Repository<MealsCategory>().GetByIdAsync(recipeDto.MealsCategoryId);
                if (mealCategory == null)
                {
                    throw new ApplicationException("Invalid MealsCategory ID");
                }

                var recipeEntity = _mapper.Map<Recipe>(recipeDto);
                recipeEntity.MealsCategory = mealCategory;

                await _unitOfWork.Repository<Recipe>().Add(recipeEntity);
                var result = await _unitOfWork.Complete();

                if (result <= 0)
                {
                    throw new ApplicationException("Failed to save recipe");
                }

                var createdDto = _mapper.Map<RecipeDto>(recipeEntity);
                createdDto.MealsCategoryName = mealCategory.CategoryName;
                return createdDto;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to add recipe: {ex.Message}", ex);
            }
        }
    }
}