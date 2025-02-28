using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications.MonthlyMembershipWithRelationsSpeci
{
    public class MonthlyMembershipWithRelationsSpecification : BaseSpecification<MonthlyMembership>
    {
        public MonthlyMembershipWithRelationsSpecification()
        {
            AddIncludes(m => m.User);
            AddIncludes(m => m.Class);
            AddIncludes(m => m.Plan);
        }

        public MonthlyMembershipWithRelationsSpecification(Expression<Func<MonthlyMembership, bool>> criteria) : base(criteria)
        {
            AddIncludes(m => m.User);
            AddIncludes(m => m.Class);
            AddIncludes(m => m.Plan);
        }
    }
}
