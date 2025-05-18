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
                throw new Exception("Failed to retrieve equipment from the database.", ex);
            }
        }

        public async Task<EquipmentViewDto> GetByIdAsync(int id)
        {
            try
            {
                var spec = new EquipmentWithRelationsSpecification(e => e.Id == id);
                var equipment = await _unitOfWork.Repository<Equipment>()
                    .GetByIdWithSpecAsync(spec);

                if (equipment == null)
                {
                    return null;
                }

                var equipmentDto = _mapper.Map<EquipmentViewDto>(equipment);

                return equipmentDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve equipment with ID {id} from the database.", ex);
            }
        }


        public async Task<ApiResponse> CreateAsync(EquipmentCreateDto equipmentCreateDto)
        {
            if (equipmentCreateDto == null)
            {
                return new ApiResponse(400, "Equipment data cannot be null.");
            }

            try
            {

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
                var existingEquipment = await _unitOfWork.Repository<Equipment>()
                    .GetByIdWithSpecAsync(spec);

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
                var equipment = await _unitOfWork.Repository<Equipment>()
                    .GetByIdWithSpecAsync(spec);

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
    }



}