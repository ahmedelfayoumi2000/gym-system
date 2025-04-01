using AutoMapper;
using GymSystem.API.DTOs.Trainer;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.Attendance;
using GymSystem.BLL.Dtos.Class;
using GymSystem.BLL.Dtos.Equipment;
using GymSystem.BLL.Dtos.GymSchedule;
using GymSystem.BLL.Dtos.MonthlyMembership;
using GymSystem.BLL.Dtos.NutritionPlan;
using GymSystem.BLL.Dtos.Offer;
using GymSystem.BLL.Dtos.Order;
using GymSystem.BLL.Dtos.Payment;
using GymSystem.BLL.Dtos.plan;
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



            CreateMap<WorkoutPlan, WorkoutPlanDto>().ReverseMap();
            CreateMap<Exercise, ExerciseDto>().ReverseMap();
            CreateMap<Feedback, FeedbackDto>().ReverseMap();
            CreateMap<BMIRecord, BMIRecordDto>().ReverseMap();
            CreateMap<MealsCategory, MealsCategoryDto>().ReverseMap();
            CreateMap<Meal, MealDto>().ReverseMap();
            CreateMap<NutritionPlan, NutritionPlanDto>().ReverseMap();
            CreateMap<Notification, NotificationDto>().ReverseMap();
            CreateMap<AppUser, UserDto>().ReverseMap();
            CreateMap<EquipmentCreateDto, Equipment>()
                .ForMember(dest => dest.LastMaintenanceDate, opt => opt.MapFrom(src => src.LastMaintenanceDate ?? DateTime.UtcNow))
                .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Count ?? 1));


            CreateMap<Equipment, EquipmentViewDto>();


            CreateMap<Repair, RepairDto>();

            CreateMap<RepairDto, Repair>()
                .ForMember(dest => dest.Time, opt => opt.MapFrom(src => src.Time ?? DateTime.UtcNow));


            CreateMap<Recipe, RecipeDto>().ReverseMap();

            CreateMap<Attendance, AttendanceDto>()
             .ForMember(dest => dest.AttendanceDate, opt => opt.MapFrom(src => src.AttendanceDate == default ? DateTime.UtcNow : src.AttendanceDate))
             .ReverseMap()
             .ForMember(dest => dest.AttendanceDate, opt => opt.MapFrom(src => src.AttendanceDate == default ? DateTime.UtcNow : src.AttendanceDate));

            CreateMap<Attendance, AttendanceViewDto>()
           .ForMember(dest => dest.CreatedByUserName, opt => opt.Ignore()); 


            CreateMap<Membership, MonthlyMembershipViewDto>()
             .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
             .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.DisplayName)) 
             .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email)) 
             .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber)) 
             .ForMember(dest => dest.Plan, opt => opt.MapFrom(src => src.Plan))
             .ForMember(dest => dest.UserCode, opt => opt.MapFrom(src => src.User.UserCode)); 


            CreateMap<MonthlyMembershipCreateDto, Membership>()
            .ForMember(dest => dest.Plan, opt => opt.Ignore())
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate ?? DateTime.UtcNow));

            CreateMap<MonthlyMembershipUpdateDto, AppUser>()
          .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
          .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.UserEmail))
          .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));

            CreateMap<MonthlyMembershipUpdateDto, Membership>();



            CreateMap<ProductCreateDto, Product>()
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()); 

            CreateMap<Product, ProductViewDto>()
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsActive)); 

            CreateMap<OrderCreateDto, Order>()
                .ForMember(dest => dest.ProductName, opt => opt.Ignore()) 
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<Order, OrderViewDto>(); 
            CreateMap<Class, ClassViewDto>()
            .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.MemberName))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
            .ForMember(dest => dest.TrainerId, opt => opt.MapFrom(src => src.TrainerId))
            .ForMember(dest => dest.Plan, opt => opt.MapFrom(src => src.Plan));

            CreateMap<ClassDto, Class>()
            .ForMember(dest => dest.Plan, opt => opt.Ignore()) 
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime ?? DateTime.Now));

            CreateMap<FinancialTransaction, TransactionDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.CreatedByUserId))
                .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => src.TransactionType.ToString()));

            CreateMap<AppUser, UserProfileDto>();

            CreateMap<UpdateProfileDto, AppUser>()
          .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<WorkoutPlan, WorkoutPlanDto>()
            .ForMember(dest => dest.WorkoutPlanId, opt => opt.MapFrom(src => src.Id));
            CreateMap<WorkoutPlanDto, WorkoutPlan>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.WorkoutPlanId));

            CreateMap<Exercise, ExerciseDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ExerciseName))
                .ForMember(dest => dest.ExerciseCategoryName, opt => opt.MapFrom(src => src.ExerciseCategory.CategoryName));
            CreateMap<ExerciseDto, Exercise>()
                .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.Name));

            CreateMap<Plan, PlanViewDto>()
                   .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.PlanName))
                   .ForMember(dest => dest.DurationInDays, opt => opt.MapFrom(src => src.DurationDays))
                   .ForMember(dest => dest.DiscountedPrice, opt => opt.MapFrom(src => src.DiscountedPrice))
                   .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(src => src.ExpireDate))
                   .ReverseMap();
            CreateMap<PlanDto, Plan>()
                .ForMember(dest => dest.HasOffer, opt => opt.Ignore())
                .ForMember(dest => dest.DiscountedPrice, opt => opt.Ignore())
                .ForMember(dest => dest.ExpireDate, opt => opt.Ignore());

            CreateMap<Plan, PlanDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.PlanName))
                .ForMember(dest => dest.DurationDays, opt => opt.MapFrom(src => src.DurationDays))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));

            CreateMap<OfferDto, Offer>().ReverseMap();

            CreateMap<Offer, OfferViewDto>()
                .ForMember(dest => dest.PlanName, opt => opt.Ignore())
                .ForMember(dest => dest.OriginalPrice, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<UpdateOffer, Offer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PlanId, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ?? true));

            CreateMap<GymScheduleDto, GymSchedule>();
            CreateMap<GymSchedule, GymScheduleViewDto>();

            CreateMap<ExerciseCategory, ExerciseCategoryDto>()
               .ForMember(dest => dest.Image, opt => opt.Ignore());

            CreateMap<ExerciseCategoryDto, ExerciseCategory>()
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted ?? false))
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());

        }
    }
}
