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
using System.Transactions;

namespace GymSystem.BLL.Repositories.Business
{
    public class FinancialTransactionRepository : IFinancialTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<FinancialTransactionRepository> _logger;

        public FinancialTransactionRepository(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IMapper mapper,
            ILogger<FinancialTransactionRepository> logger)
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
                _logger.LogWarning("Close drawer operation aborted: Request DTO is null.");
                return new ApiResponse(400, "Request data cannot be null.");
            }

            if (requestDto.StartDate > requestDto.EndDate)
            {
                _logger.LogWarning("Close drawer operation aborted: StartDate {StartDate} is after EndDate {EndDate}.", requestDto.StartDate, requestDto.EndDate);
                return new ApiResponse(400, "StartDate cannot be after EndDate.");
            }

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                _logger.LogWarning("Close drawer operation aborted: Current user ID is invalid.");
                return new ApiResponse(401, "Authenticated user ID is required.");
            }

            try
            {
                _logger.LogInformation("Initiating close drawer operation for period: {StartDate} to {EndDate}", requestDto.StartDate, requestDto.EndDate);

                if (!Guid.TryParse(currentUserId, out _))
                {
                    _logger.LogWarning("Close drawer aborted: Current user ID {CurrentUserId} is not a valid GUID.", currentUserId);
                    return new ApiResponse(401, "Current user ID must be a valid GUID.");
                }

                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    _logger.LogWarning("Close drawer aborted: Current user with ID {CurrentUserId} not found.", currentUserId);
                    return new ApiResponse(401, $"Current user with ID {currentUserId} not found.");
                }

                // Retrieve all financial transactions within the date range using a database transaction for consistency
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var transactionRepo = _unitOfWork.Repository<FinancialTransaction>();
                    var transactionSpec = new BaseSpecification<FinancialTransaction>(t =>
                        t.TransactionDate >= requestDto.StartDate &&
                        t.TransactionDate <= requestDto.EndDate &&
                        !t.IsDeleted);

                    var transactions = await transactionRepo.GetAllWithSpecAsync(transactionSpec);
                    if (transactions == null || !transactions.Any())
                    {
                        _logger.LogInformation("No financial transactions found between {StartDate} and {EndDate}.", requestDto.StartDate, requestDto.EndDate);
                        transactionScope.Complete();
                        return new ApiResponse(200, "No financial transactions found for the specified period.", new CloseDrawerResponseDto());
                    }

                    // Map transactions to DTO
                    var transactionDtos = _mapper.Map<List<TransactionDto>>(transactions);

                    // Calculate totals
                    var totalIncome = transactionDtos
                        .Where(t => t.Amount > 0) // Payments (Positive amounts)
                        .Sum(t => t.Amount);

                    var totalExpenses = Math.Abs(transactionDtos
                        .Where(t => t.Amount < 0) // Withdrawals (Negative amounts)
                        .Sum(t => t.Amount));

                    var netAmount = totalIncome - totalExpenses;

                    var responseDto = new CloseDrawerResponseDto
                    {
                        Transactions = transactionDtos,
                        TotalIncome = totalIncome,
                        TotalExpenses = totalExpenses,
                        NetAmount = netAmount
                    };

                    _logger.LogInformation("Close drawer operation completed successfully. Total Income: {TotalIncome}, Total Expenses: {TotalExpenses}, Net Amount: {NetAmount}", totalIncome, totalExpenses, netAmount);
                    transactionScope.Complete();
                    return new ApiResponse(200, "Close drawer operation completed successfully.", responseDto);
                }
            }
            catch (Exception ex) when (ex is ArgumentException or SecurityException)
            {
                _logger.LogError(ex, "Validation or security error during close drawer operation.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during close drawer operation for period: {StartDate} to {EndDate}", requestDto?.StartDate ?? DateTime.MinValue, requestDto?.EndDate ?? DateTime.MaxValue);
                return new ApiExceptionResponse(500, "An unexpected error occurred during close drawer operation.", ex.Message);
            }
        }
    }
}