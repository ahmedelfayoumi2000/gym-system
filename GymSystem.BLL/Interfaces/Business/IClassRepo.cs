using GymSystem.BLL.Dtos.Class;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.ClassWithFiltersSpec;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IClassRepo
    {
        Task<ApiResponse> AddClass(ClassDto classDto);
        Task<ApiResponse> DeleteClass(int id);
        Task<ClassViewDto> GetClass(int id);
        Task<PaginatedResult<ClassViewDto>> GetClasses(SpecPrams specParams);
        Task<ApiResponse> UpdateClass(int id, ClassDto classDto);
    }
}