using GymSystem.BLL.Dtos.Equipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GymSystem.BLL.Specifications.EquipmentSpec
{
    public class EquipmentSpecification : ISpecification<EquipmentViewDto>
    {
        private readonly string _nameFilter;
        private readonly bool? _isAvailable;

        public EquipmentSpecification(string nameFilter = null, bool? isAvailable = null)
        {
            _nameFilter = nameFilter;
            _isAvailable = isAvailable;
        }

        public Expression<Func<EquipmentViewDto, bool>> Criteria => BuildCriteria();

        public List<Expression<Func<EquipmentViewDto, object>>> Includes { get; set; } = new List<Expression<Func<EquipmentViewDto, object>>>();
        public Expression<Func<EquipmentViewDto, object>> OrderBy { get; set; }
        public Expression<Func<EquipmentViewDto, object>> OrderByDescending { get; set; }
        public int Take { get; set; }
        public int Skip { get; set; }
        public bool IsPagingEnabled { get; set; }

        private Expression<Func<EquipmentViewDto, bool>> BuildCriteria()
        {
            return equipment =>
                (_nameFilter == null || equipment.EquipmentName.Contains(_nameFilter, StringComparison.OrdinalIgnoreCase)) &&
                (_isAvailable == null || equipment.IsAvailable == _isAvailable);
        }
    }
}