using GymSystem.BLL.Dtos.Order;
using GymSystem.BLL.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IOrderRepo
    {
        Task<ApiResponse> CreateAsync(OrderCreateDto orderCreateDto, string currentUserId);
        Task<IEnumerable<OrderViewDto>> GetAllAsync();
        Task<ApiResponse> UpdateAsync(int orderId, OrderCreateDto orderCreateDto, string currentUserId);
    }
}
    