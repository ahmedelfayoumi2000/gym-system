using GymSystem.BLL.Dtos.Equipment;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Specifications;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{

    public interface IEquipmentRepo
    {
        Task<IEnumerable<EquipmentViewDto>> GetAllAsync(SpecPrams specParams = null);
        Task<EquipmentViewDto> GetByIdAsync(int id);
        Task<ApiResponse> CreateAsync(EquipmentCreateDto equipmentCreateDto);
        Task<ApiResponse> UpdateAsync(int id, EquipmentCreateDto equipmentCreateDto);
        Task<ApiResponse> DeleteAsync(int id);
    }
}