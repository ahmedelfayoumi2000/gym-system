using AutoMapper;
using GymSystem.BLL.Dtos.GymSchedule;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.GymScheduleSpec;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Enums.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories.Business
{
    public class GymScheduleRepository : IGymScheduleRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly TimeSpan GymOpeningTime = new TimeSpan(8, 0, 0); // 8:00 AM
        private readonly TimeSpan GymClosingTime = new TimeSpan(22, 0, 0); // 10:00 PM

        public GymScheduleRepository(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponse> AddSchedule(GymScheduleDto scheduleDto)
        {
            try
            {
                if (!scheduleDto.DaysOfWeek.Any())
                    return new ApiResponse(400, "At least one day of week is required.");

                if (!IsValidTimeRange(scheduleDto.StartTime, scheduleDto.EndTime))
                    return new ApiResponse(400, "Start time must be before end time and within gym operating hours.");

                // التأكد من عدم وجود تداخل في أي من الأيام
                foreach (var day in scheduleDto.DaysOfWeek)
                {
                    var overlappingSpec = new OverlappingScheduleSpecification(day, scheduleDto.StartTime, scheduleDto.EndTime);
                    var overlappingSchedule = await _unitOfWork.Repository<GymSchedule>().GetEntityWithSpecAsync(overlappingSpec);
                    if (overlappingSchedule != null)
                        return new ApiResponse(409, $"Schedule conflicts with an existing schedule on {day}.");
                }

                var scheduleEntity = _mapper.Map<GymSchedule>(scheduleDto);
                await _unitOfWork.Repository<GymSchedule>().Add(scheduleEntity);
                var result = await _unitOfWork.Complete();
                if (result <= 0)
                    return new ApiExceptionResponse(500, "Failed to add schedule due to database error.");

                var createdDto = _mapper.Map<GymScheduleViewDto>(scheduleEntity);
                return new ApiResponse(201, "Schedule added successfully", createdDto);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, $"Failed to add schedule: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateSchedule(int id, GymScheduleDto scheduleDto)
        {
            try
            {
                var existingSchedule = await _unitOfWork.Repository<GymSchedule>().GetByIdAsync(id);
                if (existingSchedule == null || !existingSchedule.IsActive)
                    return new ApiResponse(404, $"Schedule with ID {id} not found or already deleted.");

                if (!scheduleDto.DaysOfWeek.Any())
                    return new ApiResponse(400, "At least one day of week is required.");

                if (!IsValidTimeRange(scheduleDto.StartTime, scheduleDto.EndTime))
                    return new ApiResponse(400, "Start time must be before end time and within gym operating hours.");

                // التأكد من عدم التداخل باستثناء الجدول الحالي
                foreach (var day in scheduleDto.DaysOfWeek)
                {
                    var overlappingSpec = new OverlappingScheduleExcludingIdSpecification(id, day, scheduleDto.StartTime, scheduleDto.EndTime);
                    var overlappingSchedule = await _unitOfWork.Repository<GymSchedule>().GetEntityWithSpecAsync(overlappingSpec);
                    if (overlappingSchedule != null)
                        return new ApiResponse(409, $"Updated schedule conflicts with an existing schedule on {day}.");
                }

                // تحديث الأيام
                existingSchedule.DaysOfWeek.Clear();
                existingSchedule.DaysOfWeek.AddRange(scheduleDto.DaysOfWeek.Select(day => new GymScheduleDays { DayOfWeek = day }));
                existingSchedule.StartTime = scheduleDto.StartTime;
                existingSchedule.EndTime = scheduleDto.EndTime;
                existingSchedule.GroupType = scheduleDto.GroupType;

                _unitOfWork.Repository<GymSchedule>().Update(existingSchedule);
                var result = await _unitOfWork.Complete();
                if (result <= 0)
                    return new ApiExceptionResponse(500, "Failed to update schedule due to database error.");

                var updatedDto = _mapper.Map<GymScheduleViewDto>(existingSchedule);
                return new ApiResponse(200, "Schedule updated successfully", updatedDto);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, $"Failed to update schedule: {ex.Message}");
            }
        }

        // بقية الدوال (GetSchedules, GetSchedule, DeleteSchedule, GetSchedulesByDay) تحتاج تعديل بسيط للتعامل مع قايمة الأيام
        public async Task<ApiResponse> GetSchedules()
        {
            try
            {
                var spec = new AllActiveGymSchedulesSpecification();
                var schedules = await _unitOfWork.Repository<GymSchedule>().GetAllWithSpecAsync(spec);
                var scheduleDtos = _mapper.Map<IEnumerable<GymScheduleViewDto>>(schedules);
                return new ApiResponse(200, $"{scheduleDtos.Count()} Schedules retrieved successfully", scheduleDtos);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, $"Failed to retrieve schedules: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetSchedule(int id)
        {
            try
            {
                var spec = new GymScheduleByIdSpecification(id);
                var scheduleEntity = await _unitOfWork.Repository<GymSchedule>().GetEntityWithSpecAsync(spec);
                if (scheduleEntity == null)
                    return new ApiResponse(404, $"Schedule with ID {id} not found.");

                var scheduleDto = _mapper.Map<GymScheduleViewDto>(scheduleEntity);
                return new ApiResponse(200, "Schedule retrieved successfully", scheduleDto);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, $"Failed to retrieve schedule: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetSchedulesByDay(string dayOfWeek)
        {
            if (!Enum.TryParse<DayOfWeekEnum>(dayOfWeek, true, out var parsedDay))
                return new ApiResponse(400, "Invalid day of week.");

            try
            {
                var spec = new GymSchedulesByDaySpecification(parsedDay);
                var schedules = await _unitOfWork.Repository<GymSchedule>().GetAllWithSpecAsync(spec);
                var scheduleDtos = _mapper.Map<IEnumerable<GymScheduleViewDto>>(schedules);
                return new ApiResponse(200, $"Schedules for {parsedDay} retrieved successfully", scheduleDtos);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, $"Failed to retrieve schedules for {dayOfWeek}: {ex.Message}");
            }
        }

        public async Task<ApiResponse> DeleteSchedule(int id)
        {
            try
            {
                var scheduleEntity = await _unitOfWork.Repository<GymSchedule>().GetByIdAsync(id);
                if (scheduleEntity == null || !scheduleEntity.IsActive)
                {
                    return new ApiResponse(404, $"Schedule with ID {id} not found or already deleted.");
                }

                scheduleEntity.IsActive = false;
                _unitOfWork.Repository<GymSchedule>().Update(scheduleEntity);
                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiExceptionResponse(500, "Failed to delete schedule due to database error.");
                }

                return new ApiResponse(200, "Schedule deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, $"Failed to delete schedule: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetCurrentActiveSchedule()
        {
            try
            {
                var now = DateTime.Now;
                var currentDay = (DayOfWeekEnum)now.DayOfWeek;
                var currentTime = now.TimeOfDay;

                var spec = new CurrentActiveGymScheduleSpecification(currentTime, currentDay);
                var activeSchedules = await _unitOfWork.Repository<GymSchedule>().GetAllWithSpecAsync(spec);
                var scheduleDtos = _mapper.Map<IEnumerable<GymScheduleViewDto>>(activeSchedules);

                if (!scheduleDtos.Any())
                    return new ApiResponse(200, "No active schedules found for the current time.", new List<GymScheduleViewDto>());

                return new ApiResponse(200, "Current active schedule retrieved successfully", scheduleDtos.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, $"Failed to retrieve current active schedule: {ex.Message}");
            }
        }

        private bool IsValidTimeRange(TimeSpan startTime, TimeSpan endTime)
        {
            return startTime < endTime && startTime >= GymOpeningTime && endTime <= GymClosingTime;
        }
    }
}