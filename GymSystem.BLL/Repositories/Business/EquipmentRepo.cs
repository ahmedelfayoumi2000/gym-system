using AutoMapper;
using GymSystem.BLL.Dtos.Equipment;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.EquipmentSpec;
using GymSystem.DAL.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories
{
    public class EquipmentRepo : IEquipmentRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<EquipmentRepo> _logger;

        public EquipmentRepo(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<EquipmentRepo> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<IReadOnlyList<EquipmentViewDto>> GetAllAsync(SpecPrams specParams = null)
        {
            try
            {
                _logger.LogInformation("Retrieving all equipment with parameters: {@SpecParams}", specParams);

                ISpecification<Equipment> spec = specParams != null
                    ? new EquipmentWithFiltersSpecification(specParams)
                    : new EquipmentWithRelationsSpecification();

                var equipments = await _unitOfWork.Repository<Equipment>().GetAllWithSpecAsync(spec);

                var equipmentDtos = _mapper.Map<IReadOnlyList<EquipmentViewDto>>(equipments);
                _logger.LogInformation("Successfully retrieved {Count} equipment items", equipmentDtos.Count);

                return equipmentDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment list");
                throw new Exception("Failed to retrieve equipment from the database.", ex);
            }
        }

        public async Task<EquipmentViewDto> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving equipment with ID: {EquipmentId}", id);

                var spec = new EquipmentWithRelationsSpecification(e => e.Id == id);
                var equipment = await _unitOfWork.Repository<Equipment>()
                    .GetByIdWithSpecAsync(spec);

                if (equipment == null)
                {
                    _logger.LogWarning("Equipment with ID {EquipmentId} not found.", id);
                    return null;
                }

                var equipmentDto = _mapper.Map<EquipmentViewDto>(equipment);
                _logger.LogInformation("Successfully retrieved equipment with ID: {EquipmentId}", id);

                return equipmentDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment with ID: {EquipmentId}", id);
                throw new Exception($"Failed to retrieve equipment with ID {id} from the database.", ex);
            }
        }


        public async Task<ApiResponse> CreateAsync(EquipmentCreateDto equipmentCreateDto)
        {
            if (equipmentCreateDto == null)
            {
                _logger.LogWarning("Received null equipment data for creation.");
                return new ApiResponse(400, "Equipment data cannot be null.");
            }

            try
            {
                _logger.LogInformation("Creating new equipment with name: {EquipmentName}", equipmentCreateDto.EquipmentName);

                var equipment = _mapper.Map<Equipment>(equipmentCreateDto);
                await _unitOfWork.Repository<Equipment>().Add(equipment);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("Failed to save equipment with name: {EquipmentName}", equipmentCreateDto.EquipmentName);
                    return new ApiResponse(500, "Failed to save the equipment to the database.");
                }

                var createdDto = _mapper.Map<EquipmentViewDto>(equipment);
                _logger.LogInformation("Equipment created successfully with ID: {EquipmentId}", createdDto.Id);

                return new ApiResponse(201, "Equipment created successfully", createdDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating equipment with name: {EquipmentName}", equipmentCreateDto.EquipmentName);
                return new ApiExceptionResponse(500, "An error occurred while creating the equipment", ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateAsync(int id, EquipmentCreateDto equipmentCreateDto)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid equipment ID: {EquipmentId}", id);
                return new ApiResponse(400, "Equipment ID must be a positive integer.");
            }

            if (equipmentCreateDto == null)
            {
                _logger.LogWarning("Received null equipment data for update with ID: {EquipmentId}", id);
                return new ApiResponse(400, "Equipment data cannot be null.");
            }

            try
            {
                _logger.LogInformation("Updating equipment with ID: {EquipmentId}", id);

                var spec = new EquipmentWithRelationsSpecification(e => e.Id == id);
                var existingEquipment = await _unitOfWork.Repository<Equipment>()
                    .GetByIdWithSpecAsync(spec);

                if (existingEquipment == null)
                {
                    _logger.LogWarning("Equipment with ID {EquipmentId} not found for update.", id);
                    return new ApiResponse(404, $"Equipment with ID {id} not found.");
                }

                _mapper.Map(equipmentCreateDto, existingEquipment);
                _unitOfWork.Repository<Equipment>().Update(existingEquipment);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("Failed to update equipment with ID: {EquipmentId}", id);
                    return new ApiResponse(500, "Failed to update the equipment in the database.");
                }

                var updatedDto = _mapper.Map<EquipmentViewDto>(existingEquipment);
                _logger.LogInformation("Equipment updated successfully with ID: {EquipmentId}", id);

                return new ApiResponse(200, "Equipment updated successfully", updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating equipment with ID: {EquipmentId}", id);
                return new ApiExceptionResponse(500, "An error occurred while updating the equipment", ex.Message);
            }
        }


        public async Task<ApiResponse> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid equipment ID: {EquipmentId}", id);
                return new ApiResponse(400, "Equipment ID must be a positive integer.");
            }

            try
            {
                _logger.LogInformation("Deleting equipment with ID: {EquipmentId}", id);

                var spec = new EquipmentWithRelationsSpecification(e => e.Id == id);
                var equipment = await _unitOfWork.Repository<Equipment>()
                    .GetByIdWithSpecAsync(spec);

                if (equipment == null)
                {
                    _logger.LogWarning("Equipment with ID {EquipmentId} not found for deletion.", id);
                    return new ApiResponse(404, $"Equipment with ID {id} not found.");
                }

                equipment.IsDeleted = true; // Soft delete
                _unitOfWork.Repository<Equipment>().Update(equipment);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("Failed to delete equipment with ID: {EquipmentId}", id);
                    return new ApiResponse(500, "Failed to delete the equipment from the database.");
                }

                _logger.LogInformation("Equipment deleted successfully with ID: {EquipmentId}", id);
                return new ApiResponse(200, "Equipment deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting equipment with ID: {EquipmentId}", id);
                return new ApiExceptionResponse(500, "An error occurred while deleting the equipment", ex.Message);
            }
        }
    }



}