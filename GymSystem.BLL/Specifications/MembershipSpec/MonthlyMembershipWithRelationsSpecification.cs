using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;
using System.Linq.Expressions;

namespace GymSystem.BLL.Specifications.MembershipSpec
{
    public class MonthlyMembershipWithRelationsSpecification : BaseSpecification<Membership>
    {
        public MonthlyMembershipWithRelationsSpecification()
        {
            AddIncludes(m => m.User);
            AddIncludes(m => m.Plan);
        }

        public MonthlyMembershipWithRelationsSpecification(Expression<Func<Membership, bool>> criteria)
            : base(criteria)
        {
            AddIncludes(m => m.User);
            AddIncludes(m => m.Plan);
        }
    }
}