using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications.EquipmentSpec;
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
            _logger.LogInformation("Retrieving all repair records.");

            try
            {
                var repairs = await _unitOfWork.Repository<Repair>().GetAllAsync();
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
                var repairs = await _unitOfWork.Repository<Repair>().GetRepairByEquipmentIdAsync(equipmentId);
                var repairDtos = repairs?.Any() == true
                    ? _mapper.Map<List<RepairDto>>(repairs)
                    : new List<RepairDto>();

                return repairDtos;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve repairs for Equipment with ID {equipmentId}.", ex);
            }
        }

        #endregion

        #region CRUD Operations

        public async Task<ApiResponse> CreateAsync(RepairDto repairDto)
        {


            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var repair = _mapper.Map<Repair>(repairDto);
                    await _unitOfWork.Repository<Repair>().Add(repair);

                    await RecordFinancialTransaction(repair, TransactionType.Withdrawal);

                    var result = await _unitOfWork.Complete();
                    if (result <= 0)
                    {
                        return new ApiResponse(500, "Failed to save the repair record to the database.");
                    }

                    transactionScope.Complete();
                    var createdDto = _mapper.Map<RepairDto>(repair);
                    return new ApiResponse(201, "Repair record created successfully", createdDto);
                }
                catch (Exception ex)
                {
                    return new ApiExceptionResponse(500, "An error occurred while creating the repair record.", ex.Message);
                }
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task RecordFinancialTransaction(Repair repair, TransactionType transactionType)
        {
            var transaction = new FinancialTransaction
            {
                TransactionType = transactionType, 
                Amount = repair.Cost, 
                TransactionDate = DateTime.UtcNow,
                Description = $"Repair cost for Equipment ID: {repair.EquipmentId}",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.Repository<FinancialTransaction>().Add(transaction);
        }

        #endregion
    }
}