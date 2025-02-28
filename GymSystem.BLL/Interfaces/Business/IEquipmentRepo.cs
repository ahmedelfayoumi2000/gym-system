using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Specifications;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
  
    public interface IEquipmentRepo
    {
     
        Task<IReadOnlyList<EquipmentViewDto>> GetAllAsync(SpecPrams specParams = null);

      
        Task<EquipmentViewDto> GetByIdAsync(int id);

        Task<ApiResponse> CreateAsync(EquipmentCreateDto equipmentCreateDto);

        Task<ApiResponse> UpdateAsync(int id, EquipmentCreateDto equipmentCreateDto);

        Task<ApiResponse> DeleteAsync(int id);
    }
}