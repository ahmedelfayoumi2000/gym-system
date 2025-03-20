using AutoMapper;
using GymSystem.BLL.Dtos.Class;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories.Business
{
    public class ClassRepository : IClassRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ClassRepository> _logger;

        public ClassRepository(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ClassRepository> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse> AddClass(ClassDto classDto)
        {
            if (classDto == null)
            {
                _logger.LogWarning("AddClass operation aborted: ClassDto is null.");
                return new ApiResponse(400, "Class data cannot be null.");
            }

            _logger.LogInformation("Attempting to add class for member: {MemberName}", classDto.MemberName);

            try
            {
                var classSpec = new BaseSpecification<Class>(c => c.MemberName == classDto.MemberName && !c.IsDeleted);
                var existingClass = await _unitOfWork.Repository<Class>().GetEntityWithSpecAsync(classSpec);
                if (existingClass != null)
                {
                    _logger.LogWarning("Class with MemberName '{MemberName}' already exists with ID {Id}.", classDto.MemberName, existingClass.Id);
                    return new ApiResponse(409, $"Class with MemberName '{classDto.MemberName}' already exists.");
                }

                var planEntity = await _unitOfWork.Repository<Plan>().GetByIdAsync(classDto.PlanId);
                if (planEntity == null)
                {
                    _logger.LogWarning("Plan with ID {PlanId} not found.", classDto.PlanId);
                    return new ApiResponse(404, $"Plan with ID {classDto.PlanId} not found.");
                }

                var classEntity = _mapper.Map<Class>(classDto);
                classEntity.Plan = planEntity; 

                await _unitOfWork.Repository<Class>().Add(classEntity);
                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("AddClass failed: No rows affected for MemberName {MemberName}.", classDto.MemberName);
                    return new ApiExceptionResponse(500, "Failed to add class due to database error.");
                }

                var createdDto = _mapper.Map<ClassViewDto>(classEntity);
                _logger.LogInformation("Class '{MemberName}' added successfully with ID {Id}.", classDto.MemberName, classEntity.Id);
                return new ApiResponse(201, "Class added successfully", createdDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding class for MemberName: {MemberName}", classDto.MemberName);
                return new ApiExceptionResponse(500, $"Failed to add class: {ex.Message}");
            }
        }

        public async Task<ApiResponse> DeleteClass(int id)
        {
            _logger.LogInformation("Attempting to delete class with ID: {Id}", id);

            try
            {
                var classEntity = await _unitOfWork.Repository<Class>().GetByIdAsync(id);
                if (classEntity == null || classEntity.IsDeleted)
                {
                    _logger.LogWarning("Class with ID {Id} not found or already deleted.", id);
                    return new ApiResponse(404, $"Class with ID {id} not found or already deleted.");
                }

                classEntity.IsDeleted = true;
                _unitOfWork.Repository<Class>().Update(classEntity);
                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("DeleteClass failed: No rows affected for ID {Id}.", id);
                    return new ApiExceptionResponse(500, "Failed to delete class due to database error.");
                }

                _logger.LogInformation("Class with ID {Id} deleted successfully.", id);
                return new ApiResponse(200, "Class deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting class with ID: {Id}", id);
                return new ApiExceptionResponse(500, $"Failed to delete class: {ex.Message}");
            }
        }

        public async Task<ClassViewDto> GetClass(int id)
        {
            _logger.LogInformation("Retrieving class with ID: {Id}", id);

            try
            {
                var spec = new BaseSpecification<Class>(c => c.Id == id && !c.IsDeleted);
                spec.AddIncludes(c => c.Plan);
                var classEntity = await _unitOfWork.Repository<Class>().GetEntityWithSpecAsync(spec);
                if (classEntity == null)
                {
                    _logger.LogWarning("Class with ID {Id} not found.", id);
                    return null;
                }

                var classDto = _mapper.Map<ClassViewDto>(classEntity);
                _logger.LogInformation("Class with ID {Id} retrieved successfully.", id);
                return classDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving class with ID: {Id}", id);
                throw new ApplicationException($"Failed to retrieve class: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ClassViewDto>> GetClasses()
        {
            _logger.LogInformation("Retrieving all active classes.");

            try
            {
                var spec = new BaseSpecification<Class>(c => !c.IsDeleted);
                spec.AddIncludes(c => c.Plan);
                var classes = await _unitOfWork.Repository<Class>().GetAllWithSpecAsync(spec);
                var classDtos = _mapper.Map<IEnumerable<ClassViewDto>>(classes);

                _logger.LogInformation("Retrieved {Count} active classes.", classDtos.Count());
                return classDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all classes.");
                throw new ApplicationException($"Failed to retrieve classes: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse> UpdateClass(int id, ClassDto classDto)
        {
            if (classDto == null)
            {
                _logger.LogWarning("UpdateClass operation aborted for ID {Id}: ClassDto is null.", id);
                return new ApiResponse(400, "Class data cannot be null.");
            }

            _logger.LogInformation("Attempting to update class with ID: {Id}", id);

            try
            {
                var existingClass = await _unitOfWork.Repository<Class>().GetByIdAsync(id);
                if (existingClass == null || existingClass.IsDeleted)
                {
                    _logger.LogWarning("Class with ID {Id} not found or already deleted.", id);
                    return new ApiResponse(404, $"Class with ID {id} not found or already deleted.");
                }

                if (existingClass.Plan == null || existingClass.Plan.Id != classDto.PlanId)
                {
                    var newPlan = await _unitOfWork.Repository<Plan>().GetByIdAsync(classDto.PlanId);
                    if (newPlan == null)
                    {
                        _logger.LogWarning("New Plan with ID {PlanId} not found for class ID {Id}.", classDto.PlanId, id);
                        return new ApiResponse(404, $"Plan with ID {classDto.PlanId} not found.");
                    }
                    existingClass.Plan = newPlan;
                }

                _mapper.Map(classDto, existingClass);
                _unitOfWork.Repository<Class>().Update(existingClass);
                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("UpdateClass failed: No rows affected for ID {Id}.", id);
                    return new ApiExceptionResponse(500, "Failed to update class due to database error.");
                }

                var updatedDto = _mapper.Map<ClassViewDto>(existingClass);
                _logger.LogInformation("Class with ID {Id} updated successfully.", id);
                return new ApiResponse(200, "Class updated successfully", updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating class with ID: {Id}", id);
                return new ApiExceptionResponse(500, $"Failed to update class: {ex.Message}");
            }
        }
    }
}