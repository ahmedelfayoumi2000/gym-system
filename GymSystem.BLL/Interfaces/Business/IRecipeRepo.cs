using GymSystem.BLL.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IRecipeRepo
    {
        Task<IEnumerable<RecipeDto>> GetRecipesAsync();
        Task<RecipeDto> GetRecipeAsync(int id);
        Task<RecipeDto> AddRecipeAsync(RecipeDto recipeDto);
    }
}
