using AutoMapper;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.FinancialTransactionSpec;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Enums.Business;
using GymSystem.BLL.Dtos.Class;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using GymSystem.BLL.Specifications.ClassWithFiltersSpec;

namespace GymSystem.BLL.Repositories.Business
{
    public class ClassRepository : IClassRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ClassRepository> _logger;

        public ClassRepository(
            IUnitOfWork unitOfWork,
            IMapper mapper
            )
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

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var planEntity = await _unitOfWork.Repository<Plan>().GetByIdAsync(classDto.PlanId);
                    if (planEntity == null)
                    {
                        return new ApiResponse(404, "Plan not found.");
                    }

                    var classEntity = _mapper.Map<Class>(classDto);
                    classEntity.Plan = planEntity;

                    await _unitOfWork.Repository<Class>().Add(classEntity);
                    await RecordFinancialTransaction(classEntity, TransactionType.Payment);

                    var result = await _unitOfWork.Complete();
                    if (result <= 0)
                    {
                        transactionScope.Dispose();
                        return new ApiResponse(500, "Failed to save Class to the database.");
                    }

                    transactionScope.Complete();
                    var createdDto = _mapper.Map<ClassViewDto>(classEntity);

                    return new ApiResponse(201, "Class added successfully", createdDto);
                }
                catch (Exception ex)
                {
                    return new ApiResponse(500, $"Failed to add class: {ex.Message}");
                }
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
                return new ApiResponse(500, $"Failed to delete class: {ex.Message}");
            }
        }

        public async Task<ClassViewDto> GetClass(int id)
        {
            try
            {
                var spec = new ClassByIdSpecification(id);
                var classEntity = await _unitOfWork.Repository<Class>().GetByIdWithSpecAsync(spec);
                if (classEntity == null)
                {
                    _logger.LogWarning("Class with ID {Id} not found.", id);
                    return null;
                }

                var classDto = _mapper.Map<ClassViewDto>(classEntity);
                return classDto;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve class: {ex.Message}", ex);
            }
        }

        public async Task<PaginatedResult<ClassViewDto>> GetClasses(SpecPrams specParams)
        {
            try
            {
                var spec = new ClassSpecification(specParams);
                var classes = await _unitOfWork.Repository<Class>().GetAllWithSpecAsync(spec);

                var countSpec = new ClassWithFiltersForCountSpecification(specParams);
                var totalCount = await _unitOfWork.Repository<Class>().GetCountAsync(countSpec);

                var classDtos = _mapper.Map<IEnumerable<ClassViewDto>>(classes);
                return new PaginatedResult<ClassViewDto>(classDtos, totalCount, specParams);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to retrieve classes: {ex.Message}", ex);
            }
        }

        public async Task<ApiResponse> UpdateClass(int id, ClassDto classDto)
        {
            if (classDto == null)
            {
                return new ApiResponse(400, "Class data cannot be null.");
            }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var existingClass = await _unitOfWork.Repository<Class>().GetByIdAsync(id);
                    if (existingClass == null || existingClass.IsDeleted)
                    {
                        return new ApiResponse(404, $"Class with ID {id} not found.");
                    }

                    if (existingClass.Plan == null || existingClass.Plan.Id != classDto.PlanId)
                    {
                        var newPlan = await _unitOfWork.Repository<Plan>().GetByIdAsync(classDto.PlanId);
                        if (newPlan == null)
                        {
                            return new ApiResponse(404, "New plan not found.");
                        }

                        existingClass.Plan = newPlan;

                        var spec = new FinancialTransactionForClassSpecification(existingClass.MemberName, existingClass.StartTime);
                        var existingTransaction = await _unitOfWork.Repository<FinancialTransaction>().GetEntityWithSpecAsync(spec);

                        _mapper.Map(classDto, existingClass);
                        _unitOfWork.Repository<Class>().Update(existingClass);

                        await RecordFinancialTransaction(existingClass, TransactionType.Payment, existingTransaction);

                        var result = await _unitOfWork.Complete();
                        if (result <= 0)
                        {
                            transactionScope.Dispose();
                            return new ApiResponse(500, "Failed to update the Class.");
                        }

                        transactionScope.Complete();
                        var updatedDto = _mapper.Map<ClassViewDto>(existingClass);

                        return new ApiResponse(200, "Class updated successfully", updatedDto);
                    }
                    else
                    {
                        return new ApiResponse(500, $"Failed to update class");
                    }
                }
                catch (Exception ex)
                {
                    return new ApiResponse(500, $"Failed to update class: {ex.Message}");
                }
            }
        }

        private async Task RecordFinancialTransaction(Class classEntity, TransactionType transactionType, FinancialTransaction existingTransaction = null)
        {
            if (existingTransaction != null)
            {
                existingTransaction.TransactionType = transactionType;
                existingTransaction.Amount = classEntity.Plan.Price;
                existingTransaction.TransactionDate = classEntity.StartTime;
                existingTransaction.Description = $"Updated class payment for Member: {classEntity.MemberName}";
                existingTransaction.CreatedAt = DateTime.UtcNow;
                existingTransaction.IsDeleted = false;

                _unitOfWork.Repository<FinancialTransaction>().Update(existingTransaction);
            }
            else
            {
                var transaction = new FinancialTransaction
                {
                    TransactionType = transactionType,
                    Amount = classEntity.Plan.Price,
                    TransactionDate = classEntity.StartTime,
                    Description = $"Class payment for Member: {classEntity.MemberName}",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _unitOfWork.Repository<FinancialTransaction>().Add(transaction);
            }
        }
    }
}