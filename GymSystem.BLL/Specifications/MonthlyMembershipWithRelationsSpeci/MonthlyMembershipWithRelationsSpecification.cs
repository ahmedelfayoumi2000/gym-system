using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications.MonthlyMembershipWithRelationsSpeci
{
    // Specification for including User and Class relations
    public class MonthlyMembershipWithRelationsSpecification : BaseSpecification<MonthlyMembership>
    {
        public MonthlyMembershipWithRelationsSpecification() : base()
        {
            AddIncludes(m => m.User);
            AddIncludes(m => m.Class);
        }

        public MonthlyMembershipWithRelationsSpecification(Expression<Func<MonthlyMembership, bool>> criteria) : base(criteria)
        {
            AddIncludes(m => m.User);
            AddIncludes(m => m.Class);
        }
    }
}
