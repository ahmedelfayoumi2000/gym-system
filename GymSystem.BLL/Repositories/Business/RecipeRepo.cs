using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
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

                var spec = new BaseSpecification<Recipe>(r => !r.IsDeleted);
                var recipes = await _unitOfWork.Repository<Recipe>().GetAllWithSpecAsync(spec);
                var recipeDtos = _mapper.Map<IEnumerable<RecipeDto>>(recipes);

                _logger.LogInformation("Retrieved {Count} active recipes.", recipeDtos.Count());
                return recipeDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all recipes.");
                throw new ApplicationException($"Failed to retrieve recipes: {ex.Message}", ex);
            }
        }

        public async Task<RecipeDto> GetRecipeAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid recipe ID: {Id}", id);
                return null;
            }

            try
            {
                _logger.LogInformation("Retrieving recipe with ID: {Id}", id);

                var spec = new BaseSpecification<Recipe>(r => r.Id == id && !r.IsDeleted);
                var recipe = await _unitOfWork.Repository<Recipe>().GetEntityWithSpecAsync(spec);
                if (recipe == null)
                {
                    _logger.LogWarning("Recipe with ID {Id} not found.", id);
                    return null;
                }

                var recipeDto = _mapper.Map<RecipeDto>(recipe);
                _logger.LogInformation("Recipe retrieved successfully with ID: {Id}", id);
                return recipeDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recipe with ID: {Id}", id);
                throw new ApplicationException($"Failed to retrieve recipe: {ex.Message}", ex);
            }
        }
    }
}