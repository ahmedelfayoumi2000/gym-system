using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications.EquipmentSpec
{
    public class EquipmentWithFiltersSpecification : BaseSpecification<Equipment>
    {
        public EquipmentWithFiltersSpecification(SpecPrams specParams) : base()
        {
            // التصفية
            if (!string.IsNullOrEmpty(specParams.Search))
            {
                Criteria = e => e.EquipmentName.ToLower().Contains(specParams.Search.ToLower());
            }

            // الترتيب
            if (!string.IsNullOrEmpty(specParams.Sort))
            {
                switch (specParams.Sort.ToLower())
                {
                    case "name":
                        AddOrderBy(e => e.EquipmentName);
                        break;
                    case "namedesc":
                        AddOrderByDescending(e => e.EquipmentName);
                        break;
                    case "availability":
                        AddOrderBy(e => e.IsAvailable);
                        break;
                    case "availabilitydesc":
                        AddOrderByDescending(e => e.IsAvailable);
                        break;
                    default:
                        AddOrderBy(e => e.Id);
                        break;
                }
            }

            // التقسيم (Pagination)
            if (specParams.PageSize > 0)
            {
                ApplyPagination((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
            }

            // تحميل العلاقات
            AddIncludes(e => e.MaintainedByUsers);
            AddIncludes(e => e.UsedInClasses);
        }
    }
}
