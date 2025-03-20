using GymSystem.BLL.Dtos.Class;
using GymSystem.BLL.Errors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IClassRepo
    {
        Task<ApiResponse> AddClass(ClassDto classDto);

        Task<ApiResponse> UpdateClass(int id, ClassDto classDto);
        Task<ApiResponse> DeleteClass(int id);
        Task<ClassViewDto> GetClass(int id);
        Task<IEnumerable<ClassViewDto>> GetClasses();
    }
}