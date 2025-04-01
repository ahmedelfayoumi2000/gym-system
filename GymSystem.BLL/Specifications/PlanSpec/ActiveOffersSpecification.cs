using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.PlanSpec
{
    public class ActiveOffersSpecification : BaseSpecification<Offer>
    {
        public ActiveOffersSpecification()
            : base(o => o.IsActive)
        {
        }
    }
}