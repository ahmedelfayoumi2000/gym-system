using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Interfaces;
using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymSystem.BLL.Specifications.BMIRecordsForUserSpec;

namespace GymSystem.BLL.Repositories.Business
{
    public class BMIRecordRepo : IBMIRecordRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BMIRecordRepo(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse> AddBMIRecord(BMIRecordDto bmiRecordDto)
        {
            if (bmiRecordDto.WeightInKg <= 0 || bmiRecordDto.HeightInMeters <= 0)
            {
                return new ApiResponse(400, "Weight and height must be greater than 0.");
            }

            var bmiRecord = _mapper.Map<BMIRecord>(bmiRecordDto);
            var bmiValue = bmiRecord.CalculateBMI();
            bmiRecord.Category = bmiValue.DetermineBMICategory();

            await _unitOfWork.Repository<BMIRecord>().Add(bmiRecord);
            await _unitOfWork.Complete();

            return new ApiResponse(200, "BMI record added successfully", bmiRecord.Category.ToString());
        }

        public async Task<ApiResponse> DeleteBMIRecord(int id)
        {
            var existingBMIRecord = await _unitOfWork.Repository<BMIRecord>().GetByIdAsync(id);
            if (existingBMIRecord == null || existingBMIRecord.IsDeleted)
            {
                return new ApiResponse(404, "BMI record not found");
            }

            existingBMIRecord.IsDeleted = true;
            _unitOfWork.Repository<BMIRecord>().Update(existingBMIRecord);
            await _unitOfWork.Complete();

            return new ApiResponse(200, "BMI record deleted successfully");
        }

        public async Task<IEnumerable<object>> GetBMIRecordsForUser(string userId)
        {
            var spec = new BMIRecordsForUserSpecification(userId);
            var bmiRecords = await _unitOfWork.Repository<BMIRecord>().GetAllWithSpecAsync(spec);

            return bmiRecords.Select(x => new
            {
                id = x.Id,
                Category = x.Category.ToString(),
                x.MeasurementDate
            }).ToList();
        }
    }
}
