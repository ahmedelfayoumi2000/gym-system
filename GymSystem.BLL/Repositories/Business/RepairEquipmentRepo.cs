using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Specifications.EquipmentSpec;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.DAL.Entities.Enums.Business;
using System.Transactions;

namespace GymSystem.BLL.Repositories.Business
{
    public class RepairEquipmentRepo : IRepairEquipmentRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<EquipmentRepo> _logger;

        public RepairEquipmentRepo(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<EquipmentRepo> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }


        public async Task<IReadOnlyList<RepairDto>> GetAllAsync()
        {
            try
            {
                var Repairs = await _unitOfWork.Repository<Repair>().GetAllAsync();

                var repairDto = _mapper.Map<IReadOnlyList<RepairDto>>(Repairs);

                return repairDto;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve equipment from the database.", ex);
            }
        }


        public async Task<List<RepairDto>> GetRepairsByEquipmentIdAsync(int EquipmentId)
        {
            try
            {
                var repairs = await _unitOfWork.Repository<Repair>()
                    .GetRepairByEquipmentIdAsync(EquipmentId);

                if (repairs == null || !repairs.Any())
                {
                    return new List<RepairDto>();
                }

                var repairDtos = _mapper.Map<List<RepairDto>>(repairs);

                return repairDtos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve repairs for Equipment with ID {EquipmentId} from the database.", ex);
            }
        }


        public async Task<ApiResponse> CreateAsync(RepairDto RepairDto)
        {
            if (RepairDto == null)
            {
                return new ApiResponse(400, "Repair data cannot be null.");
            }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var repair = _mapper.Map<Repair>(RepairDto);
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


        #region Private Helper Methods

        private async Task RecordFinancialTransaction(Repair repair, TransactionType transactionType)
        {
            var transaction = new FinancialTransaction
            {
                TransactionType = transactionType,
                Amount = (-1) * repair.Cost,
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
