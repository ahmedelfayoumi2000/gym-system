using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.MonthlyMembershipWithRelationsSpeci;
using GymSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories
{
    public class MonthlyMembershipRepo : IMonthlyMembershipRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MonthlyMembershipRepo(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IReadOnlyList<MonthlyMembershipDto>> GetAllAsync(SpecPrams specParams = null)
        {
            try
            {
                ISpecification<MonthlyMembership> spec = specParams != null
                    ? new MonthlyMembershipWithFiltersSpecification(specParams)
                    : new MonthlyMembershipWithRelationsSpecification();

                var memberships = await _unitOfWork.Repository<MonthlyMembership>()
                    .GetAllWithSpecAsync(spec);

                var membershipDtos = memberships.Select(m => _mapper.Map<MonthlyMembershipDto>(m)).ToList();
                return membershipDtos.AsReadOnly();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve monthly memberships from the database.", ex);
            }
        }

        public async Task<MonthlyMembershipDto> GetByIdAsync(int id)
        {
            try
            {
                var spec = new MonthlyMembershipWithRelationsSpecification(m => m.Id == id);
                var membership = await _unitOfWork.Repository<MonthlyMembership>()
                    .GetByIdWithSpecAsync(spec);

                return membership == null ? null : _mapper.Map<MonthlyMembershipDto>(membership);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve monthly membership with ID {id} from the database.", ex);
            }
        }

        public async Task<ApiResponse> CreateAsync(MonthlyMembershipDto membershipDto)
        {
            if (membershipDto == null)
            {
                return new ApiResponse(400, "Monthly membership data cannot be null.");
            }

            try
            {
                var membership = _mapper.Map<MonthlyMembership>(membershipDto);
                await _unitOfWork.Repository<MonthlyMembership>().Add(membership);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to save the monthly membership to the database.");
                }

                var createdDto = _mapper.Map<MonthlyMembershipDto>(membership);
                return new ApiResponse(201, "Monthly membership created successfully", createdDto);
            }
            catch (DbUpdateException ex)
            {
                return new ApiExceptionResponse(400, "Failed to create monthly membership due to database constraints.", ex.Message);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An error occurred while creating the monthly membership", ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateAsync(int id, MonthlyMembershipDto membershipDto)
        {
            if (id <= 0)
            {
                return new ApiResponse(400, "Monthly membership ID must be a positive integer.");
            }

            if (membershipDto == null)
            {
                return new ApiResponse(400, "Monthly membership data cannot be null.");
            }

            try
            {
                var spec = new MonthlyMembershipWithRelationsSpecification(m => m.Id == id);
                var existingMembership = await _unitOfWork.Repository<MonthlyMembership>()
                    .GetByIdWithSpecAsync(spec);

                if (existingMembership == null)
                {
                    return new ApiResponse(404, $"Monthly membership with ID {id} not found.");
                }

                _mapper.Map(membershipDto, existingMembership);
                _unitOfWork.Repository<MonthlyMembership>().Update(existingMembership);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to update the monthly membership in the database.");
                }

                var updatedDto = _mapper.Map<MonthlyMembershipDto>(existingMembership);
                return new ApiResponse(200, "Monthly membership updated successfully", updatedDto);
            }
            catch (DbUpdateException ex)
            {
                return new ApiExceptionResponse(400, "Failed to update monthly membership due to database constraints.", ex.Message);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An error occurred while updating the monthly membership", ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                return new ApiResponse(400, "Monthly membership ID must be a positive integer.");
            }

            try
            {
                var spec = new MonthlyMembershipWithRelationsSpecification(m => m.Id == id);
                var membership = await _unitOfWork.Repository<MonthlyMembership>()
                    .GetByIdWithSpecAsync(spec);

                if (membership == null)
                {
                    return new ApiResponse(404, $"Monthly membership with ID {id} not found.");
                }

                _unitOfWork.Repository<MonthlyMembership>().Delete(membership);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to delete the monthly membership from the database.");
                }

                return new ApiResponse(200, "Monthly membership deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An error occurred while deleting the monthly membership", ex.Message);
            }
        }
    }


}