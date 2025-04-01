using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.OfferSpec
{
    public class ActiveOfferByPlanIdSpecification : BaseSpecification<Offer>
    {
        public ActiveOfferByPlanIdSpecification(int planId)
            : base(o => o.PlanId == planId &&
                        o.IsActive &&
                        o.StartDate <= DateTime.UtcNow &&
                        o.EndDate >= DateTime.UtcNow)
        {
        }
    }
}