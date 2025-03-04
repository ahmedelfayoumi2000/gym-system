using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.Order;
using GymSystem.BLL.Dtos.Product;
using GymSystem.BLL.Errors;


namespace GymSystem.BLL.Interfaces.Business
{
    public interface IProductRepo
    {
        Task<ApiResponse> CreateAsync(ProductCreateDto productCreateDto);
        Task<IEnumerable<ProductViewDto>> GetAllAsync();
        Task<ApiResponse> UpdateAsync(int productId, ProductCreateDto productCreateDto);

    }
}