using GymSystem.BLL.Dtos.Payment;
using GymSystem.BLL.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IPaymentService
    {
        Task<ApiResponse> CloseDrawerAsync(CloseDrawerRequestDto requestDto, string currentUserId);
    }
}
