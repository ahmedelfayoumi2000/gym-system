using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.ProductSpec
{
    public class AllProductsSpecification : BaseSpecification<Product>
    {
        public AllProductsSpecification()
            : base(p => !p.IsDeleted)
        {
        }
    }
}