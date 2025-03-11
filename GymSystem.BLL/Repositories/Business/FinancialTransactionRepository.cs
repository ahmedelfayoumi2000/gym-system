using AutoMapper;
using GymSystem.BLL.Dtos.Payment;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security;

namespace GymSystem.BLL.Repositories.Business
{
    public class FinancialTransactionRepository : IFinancialTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public FinancialTransactionRepository(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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

                if (!Guid.TryParse(currentUserId, out _))
                {
                    return new ApiResponse(401, "Current user ID must be a valid GUID.");
                }

                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    return new ApiResponse(401, $"Current user with ID {currentUserId} not found.");
                }

                var transactionRepo = _unitOfWork.Repository<FinancialTransaction>();
                var transactionSpec = new BaseSpecification<FinancialTransaction>(t =>
                    t.TransactionDate >= requestDto.StartDate &&
                    t.TransactionDate <= requestDto.EndDate &&
                    !t.IsDeleted);

                var transactions = await transactionRepo.GetAllWithSpecAsync(transactionSpec);
                if (transactions == null || !transactions.Any())
                {
                    return new ApiResponse(200, "No financial transactions found for the specified period.", new CloseDrawerResponseDto());
                }

                var transactionDtos = _mapper.Map<List<TransactionDto>>(transactions);

                // Calculate totals
                var totalIncome = transactionDtos
                    .Where(t => t.Amount > 0)
                    .Sum(t => t.Amount);

                var totalExpenses = Math.Abs(transactionDtos
                    .Where(t => t.Amount < 0)
                    .Sum(t => t.Amount));

                var netAmount = totalIncome - totalExpenses;

                var responseDto = new CloseDrawerResponseDto
                {
                    Transactions = transactionDtos,
                    TotalIncome = totalIncome,
                    TotalExpenses = totalExpenses,
                    NetAmount = netAmount
                };

                return new ApiResponse(200, "Close drawer operation completed successfully.", responseDto);
            }
            catch (Exception ex) when (ex is ArgumentException or SecurityException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An unexpected error occurred during close drawer operation.", ex.Message);
            }
        }
    }
}