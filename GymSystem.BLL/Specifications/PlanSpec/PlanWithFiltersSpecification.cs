using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications.PlanSpec
{
    public class PlanWithFiltersSpecification : BaseSpecification<Plan>
    {
        public PlanWithFiltersSpecification(SpecPrams specParams)
            : base()
        {
            ApplySearchFilter(specParams, p => p.PlanName);

            if (!string.IsNullOrEmpty(specParams.Sort))
            {
                switch (specParams.Sort.ToLower())
                {
                    case "price":
                        AddOrderBy(p => p.Price);
                        break;
                    case "pricedesc":
                        AddOrderByDescending(p => p.Price);
                        break;
                    case "duration":
                        AddOrderBy(p => p.DurationDays);
                        break;
                    case "durationdesc":
                        AddOrderByDescending(p => p.DurationDays);
                        break;
                    default:
                        AddOrderBy(p => p.Id);
                        break;
                }
            }

            ApplyPagination(specParams);
        }
    }
}
