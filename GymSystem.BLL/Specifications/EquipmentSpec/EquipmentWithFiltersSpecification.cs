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
            if (!string.IsNullOrEmpty(specParams.Search))
            {
                Criteria = e => e.EquipmentName.ToLower().Contains(specParams.Search.ToLower()) && !e.IsDeleted;
            }
            else
            {
                Criteria = e => !e.IsDeleted;
            }

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
                    case "lastmaintenance":
                        AddOrderBy(e => e.LastMaintenanceDate);
                        break;
                    case "lastmaintenancedesc":
                        AddOrderByDescending(e => e.LastMaintenanceDate);
                        break;
                    default:
                        AddOrderBy(e => e.Id);
                        break;
                }
            }

            if (specParams.PageSize > 0)
            {
                ApplyPagination((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
            }
        }

    }
}

