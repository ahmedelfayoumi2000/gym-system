using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications.MonthlyMembershipWithRelationsSpeci
{
    public class MonthlyMembershipWithRelationsSpecification : BaseSpecification<Membership>
    {
        public MonthlyMembershipWithRelationsSpecification()
        {
            AddIncludes(m => m.User);
            AddIncludes(m => m.Plan);
        }

        public MonthlyMembershipWithRelationsSpecification(Expression<Func<Membership, bool>> criteria) : base(criteria)
        {
            AddIncludes(m => m.User);
            AddIncludes(m => m.Plan);
        }
    }
}
