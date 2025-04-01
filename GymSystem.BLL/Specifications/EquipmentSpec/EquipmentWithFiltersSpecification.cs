using GymSystem.DAL.Entities;

namespace GymSystem.BLL.Specifications.EquipmentSpec
{
    public class EquipmentWithFiltersSpecification : BaseSpecification<Equipment>
    {
        public EquipmentWithFiltersSpecification(SpecPrams specParams)
            : base(e => !e.IsDeleted)
        {
            ApplySearchFilter(specParams, e => e.EquipmentName);

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

            ApplyPagination(specParams);
        }
    }
}