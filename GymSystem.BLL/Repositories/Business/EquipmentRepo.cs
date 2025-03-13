// BLL/Repositories/Business/EquipmentRepo.cs
using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.Equipment;
using GymSystem.BLL.Dtos.Product;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.EquipmentSpec;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Enums.Business;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;

namespace GymSystem.BLL.Repositories.Business
{
    public class EquipmentRepo : IEquipmentRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<EquipmentRepo> _logger;

        public EquipmentRepo(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IMapper mapper,
            ILogger<EquipmentRepo> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse> CreateAsync(EquipmentCreateDto equipmentCreateDto)
        {
            if (equipmentCreateDto == null)
            {
                return new ApiResponse(400, "Equipment data cannot be null.");
            }

            try
            {

                var spec = new BaseSpecification<Equipment>(e => e.EquipmentName == equipmentCreateDto.EquipmentName && !e.IsDeleted);
                var existingEquipment = await _unitOfWork.Repository<Equipment>().GetEntityWithSpecAsync(spec);
                if (existingEquipment != null)
                {
                    return new ApiResponse(409, $"Equipment '{equipmentCreateDto.EquipmentName}' already exists.");
                }

                var equipment = _mapper.Map<Equipment>(equipmentCreateDto);
                await _unitOfWork.Repository<Equipment>().Add(equipment);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to save the equipment to the database.");
                }

                var createdDto = _mapper.Map<EquipmentViewDto>(equipment);
                return new ApiResponse(201, "Equipment created successfully", createdDto);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An error occurred while creating the equipment", ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateAsync(int id, EquipmentCreateDto equipmentCreateDto)
        {
            if (id <= 0)
            {
                return new ApiResponse(400, "Equipment ID must be a positive integer.");
            }

            if (equipmentCreateDto == null)
            {
                return new ApiResponse(400, "Equipment data cannot be null.");
            }

            try
            {

                var spec = new EquipmentWithRelationsSpecification(e => e.Id == id);
                var existingEquipment = await _unitOfWork.Repository<Equipment>().GetEntityWithSpecAsync(spec);
                if (existingEquipment == null)
                {
                    return new ApiResponse(404, $"Equipment with ID {id} not found.");
                }

                _mapper.Map(equipmentCreateDto, existingEquipment);
                _unitOfWork.Repository<Equipment>().Update(existingEquipment);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to update the equipment in the database.");
                }

                var updatedDto = _mapper.Map<EquipmentViewDto>(existingEquipment);
                return new ApiResponse(200, "Equipment updated successfully", updatedDto);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An error occurred while updating the equipment", ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                return new ApiResponse(400, "Equipment ID must be a positive integer.");
            }

            try
            {

                var spec = new EquipmentWithRelationsSpecification(e => e.Id == id);
                var equipment = await _unitOfWork.Repository<Equipment>().GetEntityWithSpecAsync(spec);
                if (equipment == null)
                {
                    return new ApiResponse(404, $"Equipment with ID {id} not found.");
                }

                equipment.IsDeleted = true;
                _unitOfWork.Repository<Equipment>().Update(equipment);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to delete the equipment from the database.");
                }

                return new ApiResponse(200, "Equipment deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An error occurred while deleting the equipment", ex.Message);
            }
        }
        public async Task<EquipmentViewDto> GetByIdAsync(int id)
        {
            try
            {

                var spec = new BaseSpecification<Equipment>(e => e.Id == id && !e.IsDeleted);
                var equipment = await _unitOfWork.Repository<Equipment>().GetEntityWithSpecAsync(spec);
                if (equipment == null)
                {
                    return null;
                }

                var equipmentDto = _mapper.Map<EquipmentViewDto>(equipment);
                equipmentDto.MaintenanceCount = equipment.MaintainedByUsers?.Count ?? 0;
                equipmentDto.ClassUsageCount = equipment.UsedInClasses?.Count ?? 0;

                return equipmentDto;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve equipment: {ex.Message}", ex);
            }
        }

      
        public async Task<IEnumerable<EquipmentViewDto>> GetAllAsync(SpecPrams specParams = null)
        {
            try
            {
                ISpecification<Equipment> spec = specParams != null
                    ? new EquipmentWithFiltersSpecification(specParams)
                    : new EquipmentWithRelationsSpecification();

                var equipments = await _unitOfWork.Repository<Equipment>().GetAllWithSpecAsync(spec);
                var equipmentDtos = _mapper.Map<IReadOnlyList<EquipmentViewDto>>(equipments);

               
                return equipmentDtos;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve equipment: {ex.Message}", ex);
            }
        }

        #region Repair Operations

        /// <summary>
        /// Records a repair for an equipment and logs a financial transaction as a Withdrawal.
        /// </summary>
        public async Task<ApiResponse> RepairAsync(EquipmentRepairDto repairDto, string currentUserId)
        {
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var spec = new BaseSpecification<Equipment>(e => e.Id == repairDto.EquipmentId && !e.IsDeleted);
                    var equipment = await _unitOfWork.Repository<Equipment>().GetEntityWithSpecAsync(spec);
                    if (equipment == null)
                    {
                        return new ApiResponse(404, $"Equipment with ID {repairDto.EquipmentId} not found.");
                    }

                    var user = await _userManager.FindByIdAsync(currentUserId);
                    if (user == null)
                    {
                        return new ApiResponse(404, "Current user not found.");
                    }

                    var repair = new EquipmentMaintenance
                    {
                        EquipmentId = repairDto.EquipmentId,
                        UserId = currentUserId,
                        Price = repairDto.Price,
                        Description = repairDto.Description,
                        MaintenanceDate = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<EquipmentMaintenance>().Add(repair);

                    await RecordFinancialTransaction(repair, TransactionType.Withdrawal, currentUserId);

                    equipment.LastMaintenanceDate = DateTime.UtcNow;
                    equipment.IsAvailable = true;
                    _unitOfWork.Repository<Equipment>().Update(equipment);

                    var result = await _unitOfWork.Complete();
                    if (result <= 0)
                    {
                        return new ApiResponse(500, "Failed to save the repair to the database.");
                    }

                    transactionScope.Complete();
                    var updatedDto = _mapper.Map<EquipmentViewDto>(equipment);
                    updatedDto.MaintenanceCount = (equipment.MaintainedByUsers?.Count ?? 0) + 1;
                    updatedDto.ClassUsageCount = equipment.UsedInClasses?.Count ?? 0;

                    return new ApiResponse(200, "Equipment repaired successfully", updatedDto);
                }
                catch (Exception ex)
                {
                    return new ApiExceptionResponse(500, "An error occurred while repairing the equipment.", ex.Message);
                }
            }
        }

        #endregion

        #region Private Helper Methods
        private async Task RecordFinancialTransaction(EquipmentMaintenance repair, TransactionType transactionType, string userId)
        {
            var transaction = new FinancialTransaction
            {
                TransactionType = transactionType,
                Amount = repair.Price,
                TransactionDate = DateTime.UtcNow,
                Description = $"Repair cost for Equipment ID: {repair.EquipmentId}",
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.Repository<FinancialTransaction>().Add(transaction);
            _logger.LogInformation("Financial transaction recorded for repair of Equipment ID: {EquipmentId}", repair.EquipmentId);
        }

        #endregion
    }

}


