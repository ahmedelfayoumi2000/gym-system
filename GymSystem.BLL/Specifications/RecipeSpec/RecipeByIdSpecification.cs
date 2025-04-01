using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.RecipeSpec
{
    public class RecipeByIdSpecification : BaseSpecification<Recipe>
    {
        public RecipeByIdSpecification(int id)
            : base(r => r.Id == id && !r.IsDeleted)
        {
        }
    }
}