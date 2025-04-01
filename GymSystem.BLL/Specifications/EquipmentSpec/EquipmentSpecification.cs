using GymSystem.BLL.Dtos.Equipment;

namespace GymSystem.BLL.Specifications.EquipmentSpec
{
    public class EquipmentSpecification : BaseSpecification<EquipmentViewDto>
    {
        public EquipmentSpecification(string nameFilter = null, bool? isAvailable = null)
            : base(equipment =>
                (string.IsNullOrEmpty(nameFilter) || equipment.EquipmentName.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)) &&
                (isAvailable == null || equipment.IsAvailable == isAvailable))
        {
        }
    }
}