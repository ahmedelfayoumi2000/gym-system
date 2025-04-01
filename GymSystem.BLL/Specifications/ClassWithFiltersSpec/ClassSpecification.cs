using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications.ClassWithFiltersSpec
{
    public class ClassSpecification : BaseSpecification<Class>
    {
        public ClassSpecification(SpecPrams specParams)
            : base(c => !c.IsDeleted)
        {
            AddIncludes(c => c.Plan);

            ApplySearchFilter(specParams, c => c.MemberName);

            if (!string.IsNullOrEmpty(specParams.Sort))
            {
                switch (specParams.Sort.ToLower())
                {
                    case "starttime":
                        AddOrderBy(c => c.StartTime);
                        break;
                    case "starttimedesc":
                        AddOrderByDescending(c => c.StartTime);
                        break;
                    case "membername":
                        AddOrderBy(c => c.MemberName);
                        break;
                    case "membernamedesc":
                        AddOrderByDescending(c => c.MemberName);
                        break;
                    default:
                        AddOrderBy(c => c.Id);
                        break;
                }
            }
            else
            {
                AddOrderBy(c => c.Id);
            }

            ApplyPagination(specParams);
        }
    }
}
