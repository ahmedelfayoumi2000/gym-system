using GymSystem.DAL.Entities;
using System;
using System.Linq.Expressions;

namespace GymSystem.BLL.Specifications
{
    /// <summary>
    /// Specification for filtering classes based on SpecPrams, including related data.
    /// </summary>
    public class ClassWithFiltersSpecification : BaseSpecification<Class>
    {
        public ClassWithFiltersSpecification(SpecPrams specParams) : base()
        {
            if (!string.IsNullOrEmpty(specParams.Search))
            {
                Criteria = c => c.ClassName.ToLower().Contains(specParams.Search.ToLower()) && !c.IsDeleted;
            }
            else
            {
                Criteria = c => !c.IsDeleted;
            }

            if (!string.IsNullOrEmpty(specParams.Sort))
            {
                switch (specParams.Sort.ToLower())
                {
                    case "name":
                        AddOrderBy(c => c.ClassName);
                        break;
                    case "namedesc":
                        AddOrderByDescending(c => c.ClassName);
                        break;
                    case "starttime":
                        AddOrderBy(c => c.StartTime);
                        break;
                    case "starttimedesc":
                        AddOrderByDescending(c => c.StartTime);
                        break;
                    default:
                        AddOrderBy(c => c.Id);
                        break;
                }
            }

            if (specParams.PageSize > 0)
            {
                ApplyPagination((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
            }

            AddIncludes(c => c.Trainer);
        }
    }
}