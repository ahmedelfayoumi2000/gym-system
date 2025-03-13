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
    public class ClassRepository : IClassRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ClassRepository(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponse> AddClass(ClassDto classDto)
        {
            if (classDto == null)
            {
                return new ApiResponse(400, "Class data cannot be null.");
            }

            try
            {

                var classSpec = new BaseSpecification<Class>(c => c.ClassName == classDto.MemberName && !c.IsDeleted);
                var existingClass = await _unitOfWork.Repository<Class>().GetEntityWithSpecAsync(classSpec);
                if (existingClass != null)
                {
                    return new ApiResponse(409, $"Class '{classDto.MemberName}' already exists.");
                }

                var classEntity = _mapper.Map<Class>(classDto);
                await _unitOfWork.Repository<Class>().Add(classEntity);
                await _unitOfWork.Complete();
                return new ApiResponse(201, "Class added successfully", _mapper.Map<ClassDto>(classEntity));
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, $"Failed to add class: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateClass(int id, ClassDto classDto)
        {
            try
            {
                var spec = new BaseSpecification<Class>(c => c.Id == id && !c.IsDeleted);
                var existingClass = await _unitOfWork.Repository<Class>().GetEntityWithSpecAsync(spec);
                if (existingClass == null)
                {
                    return new ApiResponse(404, $"Class with ID {id} not found.");
                }

                _mapper.Map(classDto, existingClass);
                _unitOfWork.Repository<Class>().Update(existingClass);

                var result = await _unitOfWork.Complete();

                var updatedDto = _mapper.Map<ClassDto>(existingClass);
                return new ApiResponse(200, "Class updated successfully", updatedDto);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, $"Failed to update class: {ex.Message}");
            }
        }

        public async Task<ApiResponse> DeleteClass(int id)
        {
            try
            {
                var classEntity = await _unitOfWork.Repository<Class>().GetByIdAsync(id);
                if (classEntity == null || classEntity.IsDeleted)
                {
                    return new ApiResponse(404, $"Class with ID {id} not found.");
                }

                classEntity.IsDeleted = true;
                _unitOfWork.Repository<Class>().Update(classEntity);
                await _unitOfWork.Complete();
                return new ApiResponse(200, "Class deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, $"Failed to delete class: {ex.Message}");
            }
        }

        public async Task<ClassDto> GetClass(int id)
        {
            try
            {
                var spec = new BaseSpecification<Class>(c => c.Id == id && !c.IsDeleted)
                {
                    Includes = new List<Expression<Func<Class, object>>> { c => c.Trainer }
                };
                var classEntity = await _unitOfWork.Repository<Class>().GetEntityWithSpecAsync(spec);
                if (classEntity == null)
                {
                    return null;
                }

                var classDto = _mapper.Map<ClassDto>(classEntity);
                return classDto;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve class: {ex.Message}", ex);
            }
        }


        public async Task<IEnumerable<ClassDto>> GetClasses()
        {
            try
            {
                var spec = new BaseSpecification<Class>(c => !c.IsDeleted);
                var classes = await _unitOfWork.Repository<Class>().GetAllWithSpecAsync(spec);
                var classDtos = _mapper.Map<IEnumerable<ClassDto>>(classes);
                return classDtos;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve classes: {ex.Message}", ex);
            }
        }
    }
}