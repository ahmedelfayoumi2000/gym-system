using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.Equipment;
using GymSystem.BLL.Dtos.Product;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Specifications;


namespace GymSystem.BLL.Interfaces.Business
{
    public interface IEquipmentRepo
    {
        Task<ApiResponse> CreateAsync(EquipmentCreateDto equipmentCreateDto);
        Task<ApiResponse> UpdateAsync(int id, EquipmentCreateDto equipmentCreateDto);
        Task<ApiResponse> DeleteAsync(int id);
        Task<EquipmentViewDto> GetByIdAsync(int id);
        Task<IEnumerable<EquipmentViewDto>> GetAllAsync(SpecPrams specParams = null);
        Task<ApiResponse> RepairAsync(EquipmentRepairDto repairDto, string? currentUserId);

    }
}
