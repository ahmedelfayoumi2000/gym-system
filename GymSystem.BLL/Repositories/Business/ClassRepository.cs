using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories.Business
{
    /// <summary>
    /// Repository responsible for managing class-related operations in the Athletic Fit Gym system.
    /// Supports operations for the "Add Class" page in the UI.
    /// </summary>
    public class ClassRepository : IClassRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ClassRepository> _logger;

        public ClassRepository(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ClassRepository> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse> AddClass(ClassDto classDto)
        {
            if (classDto == null)
            {
                _logger.LogWarning("Attempted to add a null ClassDto.");
                return new ApiResponse(400, "Class data cannot be null.");
            }

            try
            {
                _logger.LogInformation("Attempting to add class with name: {ClassName}", classDto.ClassName);

                var classSpec = new BaseSpecification<Class>(c => c.ClassName == classDto.ClassName && !c.IsDeleted);
                var existingClass = await _unitOfWork.Repository<Class>().GetEntityWithSpecAsync(classSpec);
                if (existingClass != null)
                {
                    _logger.LogWarning("Class with name {ClassName} already exists.", classDto.ClassName);
                    return new ApiResponse(409, $"Class '{classDto.ClassName}' already exists.");
                }

                var classEntity = _mapper.Map<Class>(classDto);
                await _unitOfWork.Repository<Class>().Add(classEntity);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("Failed to save class with name: {ClassName}", classDto.ClassName);
                    return new ApiResponse(500, "Failed to save the class to the database.");
                }

                var createdDto = _mapper.Map<ClassDto>(classEntity);
                _logger.LogInformation("Class {ClassName} added successfully with ID: {Id}", classDto.ClassName, classEntity.Id);
                return new ApiResponse(201, "Class added successfully", createdDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding class with name: {ClassName}", classDto.ClassName);
                return new ApiExceptionResponse(500, $"Failed to add class: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateClass(int id, ClassDto classDto)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid class ID: {Id}", id);
                return new ApiResponse(400, "Class ID must be a positive integer.");
            }

            if (classDto == null)
            {
                _logger.LogWarning("Attempted to update class with ID {Id} using null ClassDto.", id);
                return new ApiResponse(400, "Class data cannot be null.");
            }

            try
            {
                _logger.LogInformation("Attempting to update class with ID: {Id}", id);

                var spec = new BaseSpecification<Class>(c => c.Id == id && !c.IsDeleted);
                var existingClass = await _unitOfWork.Repository<Class>().GetEntityWithSpecAsync(spec);
                if (existingClass == null)
                {
                    _logger.LogWarning("Class with ID {Id} not found or already deleted.", id);
                    return new ApiResponse(404, $"Class with ID {id} not found.");
                }

                _mapper.Map(classDto, existingClass);
                _unitOfWork.Repository<Class>().Update(existingClass);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("Failed to update class with ID: {Id}", id);
                    return new ApiResponse(500, "Failed to update the class in the database.");
                }

                var updatedDto = _mapper.Map<ClassDto>(existingClass);
                _logger.LogInformation("Class with ID {Id} updated successfully.", id);
                return new ApiResponse(200, "Class updated successfully", updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating class with ID: {Id}", id);
                return new ApiExceptionResponse(500, $"Failed to update class: {ex.Message}");
            }
        }

        public async Task<ApiResponse> DeleteClass(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid class ID: {Id}", id);
                return new ApiResponse(400, "Class ID must be a positive integer.");
            }

            try
            {
                _logger.LogInformation("Attempting to delete class with ID: {Id}", id);

                var spec = new BaseSpecification<Class>(c => c.Id == id && !c.IsDeleted);
                var classEntity = await _unitOfWork.Repository<Class>().GetEntityWithSpecAsync(spec);
                if (classEntity == null)
                {
                    _logger.LogWarning("Class with ID {Id} not found or already deleted.", id);
                    return new ApiResponse(404, $"Class with ID {id} not found.");
                }

                classEntity.IsDeleted = true;
                _unitOfWork.Repository<Class>().Update(classEntity);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("Failed to delete class with ID: {Id}", id);
                    return new ApiResponse(500, "Failed to delete the class from the database.");
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

        public async Task<ClassDto> GetClass(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving class with ID: {Id}", id);

                var spec = new BaseSpecification<Class>(c => c.Id == id && !c.IsDeleted)
                {
                    Includes = new List<Expression<Func<Class, object>>> { c => c.Trainer }
                };
                var classEntity = await _unitOfWork.Repository<Class>().GetEntityWithSpecAsync(spec);
                if (classEntity == null)
                {
                    _logger.LogWarning("Class with ID {Id} not found.", id);
                    return null;
                }

                var classDto = _mapper.Map<ClassDto>(classEntity);
                _logger.LogInformation("Class with ID {Id} retrieved successfully.", id);
                return classDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving class with ID: {Id}", id);
                throw new ApplicationException($"Failed to retrieve class: {ex.Message}", ex);
            }
        }

     
        public async Task<IEnumerable<ClassDto>> GetClasses(SpecPrams specParams = null)
        {
            try
            {
                _logger.LogInformation("Retrieving all active classes with parameters: {@SpecParams}", specParams);

                ISpecification<Class> spec;
                if (specParams != null)
                {
                    spec = new ClassWithFiltersSpecification(specParams);
                }
                else
                {
                    spec = new ClassDefaultSpecification(c => !c.IsDeleted);
                }

                var classes = await _unitOfWork.Repository<Class>().GetAllWithSpecAsync(spec);
                var classDtos = _mapper.Map<IEnumerable<ClassDto>>(classes);

                _logger.LogInformation("Retrieved {Count} active classes.", classDtos.Count());
                return classDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all classes.");
                throw new ApplicationException($"Failed to retrieve classes: {ex.Message}", ex);
            }
        }
    }
}