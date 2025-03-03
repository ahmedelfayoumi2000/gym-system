using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications.MonthlyMembershipWithRelationsSpeci
{
    // Specification for filtering memberships
    public class MonthlyMembershipWithFiltersSpecification : BaseSpecification<MonthlyMembership>
    {
        public MonthlyMembershipWithFiltersSpecification(SpecPrams specParams) : base()
        {
            if (!string.IsNullOrEmpty(specParams.Search))
            {
                Criteria = m => (m.User.DisplayName.ToLower().Contains(specParams.Search.ToLower()) ||
                    m.Class.ClassName.ToLower().Contains(specParams.Search.ToLower())) && !m.IsDeleted;
            }
            else
            {
                Criteria = m => !m.IsDeleted;
            }

            AddIncludes(m => m.User);
            AddIncludes(m => m.Class);

            if (specParams.PageSize > 0)
            {
                ApplyPagination((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
            }
        }
    }
}
