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
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

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

        // دالة الإضافة: بنضيف معدة جديدة
        // : لما الفرونت يبعتلي بيانات معدة جديدة، بشوف لو موجودة قبل كدا، لو لأ، بضيفها وأرجع الـ DTO بتاعها
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

        // دالة التعديل: بنعدل معدة موجودة
      
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

                var spec = new BaseSpecification<Equipment>(e => e.Id == id && !e.IsDeleted);
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

                var spec = new BaseSpecification<Equipment>(e => e.Id == id && !e.IsDeleted);
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

        // دالة جلب معدة معينة: برجع تفاصيل معدة بناءً على الـ ID
        //  الفرونت بيبعتلي رقم المعدة، بجيب كل حاجة عنها وأرجعهاله
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
                _logger.LogInformation("بيجيب كل المعدات مع الفلاتر: {@SpecParams}", specParams);

                var spec = specParams != null
                    ? new EquipmentWithFiltersSpecification(specParams)
                    : new BaseSpecification<Equipment>(e => !e.IsDeleted);

                var equipments = await _unitOfWork.Repository<Equipment>().GetAllWithSpecAsync(spec);
                var equipmentDtos = _mapper.Map<IEnumerable<EquipmentViewDto>>(equipments);

                foreach (var dto in equipmentDtos)
                {
                    var entity = equipments.First(e => e.Id == dto.Id);
                    dto.MaintenanceCount = entity.MaintainedByUsers?.Count ?? 0;
                    dto.ClassUsageCount = entity.UsedInClasses?.Count ?? 0;
                }

                _logger.LogInformation("جبت {Count} معدة بنجاح.", equipmentDtos.Count());
                return equipmentDtos;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve equipment: {ex.Message}", ex);
            }
        }

        // دالة الإصلاح: بتسجل عملية صيانة لمعدة وبتحدث تاريخ الصيانة
        //  الفرونت بيبعت رقم المعدة وسعر ووصف الإصلاح، بضيف السجل ده وبحدث المعدة
        public async Task<ApiResponse> RepairAsync(EquipmentRepairDto repairDto, string? currentUserId)
        {
            if (repairDto == null || repairDto.EquipmentId <= 0)
            {
                return new ApiResponse(400, "Repair data or Equipment ID is invalid.");
            }

            try
            {

                var spec = new BaseSpecification<Equipment>(e => e.Id == repairDto.EquipmentId && !e.IsDeleted);
                var equipment = await _unitOfWork.Repository<Equipment>().GetEntityWithSpecAsync(spec);
                if (equipment == null)
                {
                    _logger.LogWarning("المعدة رقم {EquipmentId} مش موجودة.", repairDto.EquipmentId);
                    return new ApiResponse(404, $"Equipment with ID {repairDto.EquipmentId} not found.");
                }

                var user = await _userManager.FindByIdAsync(currentUserId);
                if (user == null)
                {
                    _logger.LogWarning("المستخدم برقم {UserId} مش موجود.", currentUserId);
                    return new ApiResponse(400, "Current user not found.");
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

                equipment.LastMaintenanceDate = DateTime.UtcNow;
                equipment.IsAvailable = true; // لما تتصلح، ترجع متاحة
                _unitOfWork.Repository<Equipment>().Update(equipment);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to save the repair to the database.");
                }

                var updatedDto = _mapper.Map<EquipmentViewDto>(equipment);
                updatedDto.MaintenanceCount = equipment.MaintainedByUsers?.Count + 1 ?? 1; 
                updatedDto.ClassUsageCount = equipment.UsedInClasses?.Count ?? 0;

                return new ApiResponse(200, "Equipment repaired successfully", updatedDto);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An error occurred while repairing the equipment", ex.Message);
            }
        }

      

    }
}

