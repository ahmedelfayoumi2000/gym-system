using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IRepairEquipmentRepo
    {
        Task<IReadOnlyList<RepairDto>> GetAllAsync();


        Task<List<RepairDto>> GetRepairsByEquipmentIdAsync(int id);

        Task<ApiResponse> CreateAsync(RepairDto RepairDto, string currentUserId);
    }
}
