using AutoMapper;
using GymSystem.API.DTOs.Trainer;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.Attendance;
using GymSystem.BLL.Dtos.Class;
using GymSystem.BLL.Dtos.Equipment;
using GymSystem.BLL.Dtos.MonthlyMembership;
using GymSystem.BLL.Dtos.NutritionPlan;
using GymSystem.BLL.Dtos.Order;
using GymSystem.BLL.Dtos.Payment;
using GymSystem.BLL.Dtos.Product;
using GymSystem.BLL.Dtos.Trainer;
using GymSystem.BLL.Dtos.User;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Identity;

namespace GymSystem.API.Helpers
{
    public class MappingProfiles : Profile
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
                .ForMember(dest => dest.IsStopped, opt => opt.MapFrom(src => src.IsStopped))
                //.ForMember(dest => dest.HaveDays, opt => opt.MapFrom(src => src.HaveDays))
                .ForMember(dest => dest.AddBy, opt => opt.MapFrom(src => src.AddBy))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                   .ForMember(dest => dest.Salary, opt => opt.MapFrom(src => src.Salary));


            // Map from CreateTrainerDto to AppUser
            CreateMap<CreateTrainerDto, AppUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                 .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.Age))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.UserRole, opt => opt.MapFrom(src => 2)) // Trainer Role
                .ForMember(dest => dest.Salary, opt => opt.MapFrom(src => src.Salary));

            // Map from UpdateTrainerDto to AppUser
            CreateMap<UpdateTrainerDto, AppUser>()
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                 .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.Age))
               .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
               .ForMember(dest => dest.Salary, opt => opt.MapFrom(src => src.Salary));
            //====================================================================================


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
            CreateMap<EquipmentCreateDto, Equipment>();
            CreateMap<Equipment, EquipmentViewDto>();
            CreateMap<Repair, RepairDto>().ReverseMap();

            CreateMap<Attendance, AttendanceDto>()
             .ForMember(dest => dest.AttendanceDate, opt => opt.MapFrom(src => src.AttendanceDate == default ? DateTime.UtcNow : src.AttendanceDate))
             .ReverseMap()
             .ForMember(dest => dest.AttendanceDate, opt => opt.MapFrom(src => src.AttendanceDate == default ? DateTime.UtcNow : src.AttendanceDate));


            CreateMap<MonthlyMembership, MonthlyMembershipViewDto>()
             .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
             .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName)) 
             .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
             .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber)) 
             .ForMember(dest => dest.Plan, opt => opt.MapFrom(src => src.Plan)) 
                                                                               
             .ForMember(dest => dest.UserCode, opt => opt.MapFrom(src => src.User.UserCode));


            CreateMap<MonthlyMembershipCreateDto, MonthlyMembership>()
            .ForMember(dest => dest.Plan, opt => opt.Ignore())
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate ?? DateTime.UtcNow));

            CreateMap<PlanDto, Plan>().ReverseMap();

            CreateMap<ProductCreateDto, Product>()
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()); 

            CreateMap<Product, ProductViewDto>()
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsActive));

            CreateMap<OrderCreateDto, Order>()
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<Order, OrderViewDto>();
            //CreateMap<Payment, PaymentDto>();

            CreateMap<Class, ClassViewDto>()
            .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.MemberName))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
            .ForMember(dest => dest.TrainerId, opt => opt.MapFrom(src => src.TrainerId))
            .ForMember(dest => dest.Plan, opt => opt.MapFrom(src => src.Plan));

            CreateMap<ClassDto, Class>()
            .ForMember(dest => dest.Plan, opt => opt.Ignore())
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime ?? DateTime.Now));

            CreateMap<FinancialTransaction, TransactionDto>()
                .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => src.TransactionType.ToString()));
        }
    }
}
