using AutoMapper;
using GymSystem.BLL.Dtos.Payment;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security;
namespace GymSystem.BLL.Repositories.Business
{
    public class PaymentRepository : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentRepository> _logger;

        public PaymentRepository(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IMapper mapper,
            ILogger<PaymentRepository> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

    
        public async Task<ApiResponse> CloseDrawerAsync(CloseDrawerRequestDto requestDto, string currentUserId)
        {
            if (requestDto == null)
            {
                return new ApiResponse(400, "Request data cannot be null.");
            }

            if (requestDto.StartDate > requestDto.EndDate)
            {
                return new ApiResponse(400, "StartDate cannot be after EndDate.");
            }

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return new ApiResponse(401, "Authenticated user ID is required.");
            }

            try
            {
                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    _logger.LogWarning("Close drawer aborted: Current user with ID {CurrentUserId} not found.", currentUserId);
                    return new ApiResponse(401, $"Current user with ID {currentUserId} not found.");
                }

                var paymentRepo = _unitOfWork.Repository<Payment>();
                var paymentSpec = new BaseSpecification<Payment>(p =>
                    p.PaymentDate >= requestDto.StartDate &&
                    p.PaymentDate <= requestDto.EndDate &&
                    !p.IsDeleted);

                var payments = await paymentRepo.GetAllWithSpecAsync(paymentSpec);
                if (payments == null || !payments.Any())
                {
                    return new ApiResponse(200, $"No payments found for the specified period between { requestDto.StartDate} and {requestDto.EndDate}.",
                        new CloseDrawerResponseDto());
                }

                var paymentDtos = _mapper.Map<List<PaymentDto>>(payments);
                var totalAmount = paymentDtos.Sum(p => p.Amount);

                var responseDto = new CloseDrawerResponseDto
                {
                    Payments = paymentDtos,
                    TotalAmount = totalAmount
                };

                return new ApiResponse(200, "Close drawer operation completed successfully.", responseDto);
            }
            catch (Exception ex) when (ex is ArgumentException or SecurityException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, $"An unexpected error occurred during close drawer operation" +
                    $"  for period: { requestDto?.StartDate ?? DateTime.MinValue} to {requestDto?.EndDate ?? DateTime.MaxValue}.", ex.Message);
            }
        }
    }
}