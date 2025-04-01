using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.RecipeSpec
{
    public class AllRecipesSpecification : BaseSpecification<Recipe>
    {
        public AllRecipesSpecification()
            : base(r => !r.IsDeleted)
        {
        }
    }
}