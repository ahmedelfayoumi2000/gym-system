using GymSystem.BLL.Dtos.User;
using GymSystem.BLL.Errors;
using GymSystem.DAL.Entities.Identity;

namespace GymSystem.BLL.Interfaces.Auth
{
    public interface IAccountService
    {
        Task<ApiResponse> RegisterAsync(Register user, Func<string, string, string> generateCallBackUrl);
        Task<ApiResponse> LoginAsync(Login dto);
        Task<ApiResponse> ForgetPassword(string email);
        ApiResponse VerfiyOtp(VerifyOtp dto);
        Task SendEmailAsync(string To, string Subject, string Body, CancellationToken Cancellation = default);
        Task<(bool Succeeded, string ErrorMessage)> ConfirmUserEmailAsync(string userId, string token);
        Task<ApiResponse> ResetPasswordAsync(ResetPassword dto);
        Task<ApiResponse> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
        Task<ApiResponse> ResendConfirmationEmailAsync(string email, Func<string, string, string> generateCallBackUrl);
        Task<ApiResponse> LogoutAsync(string userId); 
    }
}