using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Enums.Business;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

namespace GymSystem.BLL.Repositories.Business
{
    public class RepairEquipmentRepo : IRepairEquipmentRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RepairEquipmentRepo> _logger;

        public RepairEquipmentRepo(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<RepairEquipmentRepo> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Retrieval Methods

        public async Task<IReadOnlyList<RepairDto>> GetAllAsync()
        {
            try
            {
                var spec = new BaseSpecification<Repair>(r => true)
                {
                    Includes = new List<System.Linq.Expressions.Expression<Func<Repair, object>>>
                    {
                        r => r.Equipment
                    }
                };
                var repairs = await _unitOfWork.Repository<Repair>().GetAllWithSpecAsync(spec);
                var repairDtos = _mapper.Map<IReadOnlyList<RepairDto>>(repairs);
                _logger.LogInformation("Retrieved {Count} repair records.", repairDtos.Count);
                return repairDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving repair records.");
                throw new ApplicationException("Failed to retrieve repair records from the database.", ex);
            }
        }

        public async Task<List<RepairDto>> GetRepairsByEquipmentIdAsync(int equipmentId)
        {
            try
            {
                var spec = new BaseSpecification<Repair>(r => r.EquipmentId == equipmentId);
                var repairs = await _unitOfWork.Repository<Repair>().GetAllWithSpecAsync(spec);
                var repairDtos = repairs?.Any() == true
                    ? _mapper.Map<List<RepairDto>>(repairs)
                    : new List<RepairDto>();
                _logger.LogInformation("Retrieved {Count} repairs for Equipment ID {EquipmentId}.", repairDtos.Count, equipmentId);
                return repairDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve repairs for Equipment with ID {EquipmentId}.", equipmentId);
                throw new ApplicationException($"Failed to retrieve repairs for Equipment with ID {equipmentId}.", ex);
            }
        }

        #endregion

        #region CRUD Operations

        public async Task<ApiResponse> CreateAsync(RepairDto repairDto, string currentUserId)
        {
            if (string.IsNullOrEmpty(currentUserId))
            {
                return new ApiResponse(401, "User authentication required.");
            }

            var equipmentSpec = new BaseSpecification<Equipment>(e => e.Id == repairDto.EquipmentId);
            var equipment = await _unitOfWork.Repository<Equipment>().GetEntityWithSpecAsync(equipmentSpec);
            if (equipment == null)
            {
                return new ApiResponse(404, $"Equipment with ID {repairDto.EquipmentId} not found.");
            }

            // Ensure DbContext uses a single connection for the transaction
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var repair = _mapper.Map<Repair>(repairDto);
                    await _unitOfWork.Repository<Repair>().Add(repair);

                    await RecordFinancialTransaction(repair, TransactionType.Withdrawal, currentUserId);

                    var result = await _unitOfWork.Complete();
                    if (result <= 0)
                    {
                        _logger.LogError("Failed to save the repair record for Equipment ID: {EquipmentId}.", repairDto.EquipmentId);
                        return new ApiResponse(500, "Failed to save the repair record to the database.");
                    }

                    transactionScope.Complete();
                    var createdDto = _mapper.Map<RepairDto>(repair);
                    _logger.LogInformation("Repair record created successfully for Equipment ID: {EquipmentId}.", repairDto.EquipmentId);
                    return new ApiResponse(201, "Repair record created successfully", createdDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating repair record for Equipment ID: {EquipmentId}.", repairDto.EquipmentId);
                    return new ApiExceptionResponse(500, "An error occurred while creating the repair record.", ex.Message);
                }
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task RecordFinancialTransaction(Repair repair, TransactionType transactionType, string userId)
        {
            var transaction = new FinancialTransaction
            {
                TransactionType = transactionType,
                Amount = -repair.Cost, // Negative for Withdrawal
                TransactionDate = DateTime.UtcNow,
                Description = $"Repair cost for Equipment ID: {repair.EquipmentId}",
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.Repository<FinancialTransaction>().Add(transaction);
        }

        #endregion
    }
}