using AutoMapper;
using GymSystem.API.DTOs.Trainer;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.Equipment;
using GymSystem.BLL.Dtos.NutritionPlan;
using GymSystem.BLL.Dtos.Trainer;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Identity;

namespace GymSystem.API.Helpers
{
    public class MappingProfiles :Profile
    {
        public MappingProfiles()
        {
            // Map from AppUser to TrainerDto
            CreateMap<AppUser, TrainerDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.Age))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.IsStopped, opt => opt.MapFrom(src => src.IsStopped))
                .ForMember(dest => dest.HaveDays, opt => opt.MapFrom(src => src.HaveDays))
                .ForMember(dest => dest.AddBy, opt => opt.MapFrom(src => src.AddBy))
				.ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
				.ForMember(dest => dest.RemainingDays, opt => opt.MapFrom(src => src.RemainingDays));


            // Map from CreateTrainerDto to AppUser
            CreateMap<CreateTrainerDto, AppUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
				 .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.Age))
				.ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.UserRole, opt => opt.MapFrom(src => 2)); // Trainer Role

            // Map from UpdateTrainerDto to AppUser
            CreateMap<UpdateTrainerDto, AppUser>()
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
				 .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.Age))
				.ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
               .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address));
			//====================================================================================


			CreateMap<Attendance, DailyAttendanceDto>().ReverseMap();
            CreateMap<Class, ClassDto>().ReverseMap();
            CreateMap<ExerciseCategory, ExerciseCategoryDto>().ReverseMap();
            CreateMap<WorkoutPlan, WorkoutPlanDto>().ReverseMap();
            CreateMap<Exercise, ExerciseDto>().ReverseMap();
            CreateMap<Feedback, FeedbackDto>().ReverseMap();
            CreateMap<BMIRecord, BMIRecordDto>().ReverseMap();
            CreateMap<MealsCategory, MealsCategoryDto>().ReverseMap();
            CreateMap<Meal, MealDto>().ReverseMap();
            CreateMap<NutritionPlan, NutritionPlanDto>().ReverseMap();
            CreateMap<Membership, MembershipDto>().ReverseMap();
            CreateMap<Notification, NotificationDto>().ReverseMap();
            CreateMap<AppUser, UserDto>().ReverseMap();
            //CreateMap<EquipmentDto, Equipment>().ReverseMap();
            CreateMap<EquipmentCreateDto, Equipment>();
            CreateMap<Equipment, EquipmentViewDto>();
        }
    }
}
