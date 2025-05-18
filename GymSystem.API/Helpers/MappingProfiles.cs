using AutoMapper;
using GymSystem.API.DTOs.Trainer;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.Attendance;
using GymSystem.BLL.Dtos.Class;
using GymSystem.BLL.Dtos.Dashboard;
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
using GymSystem.BLL.Interfaces.Business;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Identity;
using MailKit.Search;
using StackExchange.Redis;

namespace GymSystem.API.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
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

            CreateMap<UpdateTrainerDto, AppUser>()
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                 .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.Age))
               .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
               .ForMember(dest => dest.Salary, opt => opt.MapFrom(src => src.Salary));
            //====================================================================================



            CreateMap<WorkoutPlan, WorkoutPlanDto>()
                .ForMember(dest => dest.WorkoutPlanId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.PlanName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.TrainerId, opt => opt.MapFrom(src => src.TrainerId))
                .ForMember(dest => dest.MembershipId, opt => opt.MapFrom(src => src.MembershipId))
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.DayOfWeek))
                .ForMember(dest => dest.Exercises, opt => opt.MapFrom(src => src.Exercises));

            CreateMap<WorkoutPlanDto, WorkoutPlan>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.WorkoutPlanId ?? 0))
                .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.PlanName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.TrainerId, opt => opt.MapFrom(src => src.TrainerId))
                .ForMember(dest => dest.MembershipId, opt => opt.MapFrom(src => src.MembershipId))
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.DayOfWeek))
                .ForMember(dest => dest.Exercises, opt => opt.Ignore());


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
             .ForMember(dest => dest.UserCode, opt => opt.MapFrom(src => src.User.UserCode))
             .ForMember(dest => dest.Goal, opt => opt.MapFrom(src => src.User.Goal))
             .ForMember(dest => dest.FitnessLevel, opt => opt.MapFrom(src => src.User.FitnessLevel));


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

            CreateMap<OrderCreateDto, DAL.Entities.Order>()
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<DAL.Entities.Order, OrderViewDto>();
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
            CreateMap<UpdateProfileDto, AppUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) 
                .ForMember(dest => dest.DisplayName, opt =>
                {
                    opt.Condition(src => !string.IsNullOrWhiteSpace(src.FullName)); 
                    opt.MapFrom(src => src.FullName);
                })
                .ForMember(dest => dest.UserName, opt =>
                {
                    opt.Condition(src => !string.IsNullOrWhiteSpace(src.Email)); 
                    opt.MapFrom(src => src.Email);
                })
                .ForMember(dest => dest.Email, opt =>
                {
                    opt.Condition(src => !string.IsNullOrWhiteSpace(src.Email)); 
                    opt.MapFrom(src => src.Email);
                })
                .ForMember(dest => dest.Gender, opt =>
                {
                    opt.Condition(src => src.Gender.HasValue); 
                    opt.MapFrom(src => src.Gender);
                })
                .ForMember(dest => dest.Age, opt =>
                {
                    opt.Condition(src => src.Age.HasValue);
                    opt.MapFrom(src => src.Age.HasValue ? (uint?)src.Age.Value : null);
                })
                .ForMember(dest => dest.Weight, opt =>
                {
                    opt.Condition(src => src.Weight.HasValue);
                    opt.MapFrom(src => src.Weight);
                })
                .ForMember(dest => dest.Height, opt =>
                {
                    opt.Condition(src => src.Height.HasValue);
                    opt.MapFrom(src => src.Height);
                })
                .ForMember(dest => dest.ProfileImageName, opt =>
                {
                    opt.Condition(src => !string.IsNullOrWhiteSpace(src.ImageUrl));
                    opt.MapFrom(src => src.ImageUrl);
                });

            CreateMap<AppUser, UserProfileDto>()
              .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.DisplayName))
              .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ProfileImageName))
              .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.Age.HasValue ? src.Age : 0))
              .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Weight ?? 0f))
              .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height ?? 0f))
              .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
              .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
              .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
              .ForMember(dest => dest.Goal, opt => opt.MapFrom(src => src.Goal))
              .ForMember(dest => dest.FitnessLevel, opt => opt.MapFrom(src => src.FitnessLevel))
              .ForMember(dest => dest.IsProfileConfirmed, opt => opt.MapFrom(src => src.IsProfileConfirmed ?? false)); // تحويل bool? لـ bool

            CreateMap<WorkoutPlan, WorkoutPlanDto>()
                .ForMember(dest => dest.WorkoutPlanId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.PlanName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.TrainerId, opt => opt.MapFrom(src => src.TrainerId))
                .ForMember(dest => dest.MembershipId, opt => opt.MapFrom(src => src.MembershipId))
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.DayOfWeek))
                .ForMember(dest => dest.Exercises, opt => opt.MapFrom(src => src.Exercises))
                .ForMember(dest => dest.ExercisesCount, opt => opt.MapFrom(src => src.Exercises != null ? src.Exercises.Count : 0));

            CreateMap<WorkoutPlanDto, WorkoutPlan>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.PlanName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.TrainerId, opt => opt.MapFrom(src => src.TrainerId))
                .ForMember(dest => dest.MembershipId, opt => opt.MapFrom(src => src.MembershipId))
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.DayOfWeek))
                .ForMember(dest => dest.Exercises, opt => opt.Ignore());

            CreateMap<Exercise, ExerciseDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ExerciseName))
                .ForMember(dest => dest.ExerciseCategoryName, opt => opt.MapFrom(src => src.ExerciseCategory.CategoryName))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.Image, opt => opt.Ignore());
            CreateMap<ExerciseDto, Exercise>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Id, opt => opt.Ignore());

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

            CreateMap<GymScheduleDto, GymSchedule>()
                .ForMember(dest => dest.DaysOfWeek, opt => opt.MapFrom(src => src.DaysOfWeek.Select(day => new GymScheduleDays { DayOfWeek = day }).ToList()))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
                .ForMember(dest => dest.GroupType, opt => opt.MapFrom(src => src.GroupType));

            CreateMap<GymSchedule, GymScheduleViewDto>()
                .ForMember(dest => dest.DaysOfWeek, opt => opt.MapFrom(src => src.DaysOfWeek.Select(d => d.DayOfWeek).ToList()))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
                .ForMember(dest => dest.GroupType, opt => opt.MapFrom(src => src.GroupType))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            CreateMap<ExerciseCategory, ExerciseCategoryDto>()
               .ForMember(dest => dest.Image, opt => opt.Ignore());

            CreateMap<ExerciseCategoryDto, ExerciseCategory>()
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted ?? false))
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());

            CreateMap<AppUser, EmployeeDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PassWord, opt => opt.Ignore())
                .ForMember(dest => dest.UserRole, opt => opt.MapFrom(src => src.UserRole))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.Salary, opt => opt.MapFrom(src => src.Salary));

            CreateMap<EmployeeDto, AppUser>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
               .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
               .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(dest => dest.UserRole, opt => opt.MapFrom(src => src.UserRole))
               .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
               .ForMember(dest => dest.Salary, opt => opt.MapFrom(src => src.Salary));

            CreateMap<UserDailyStats, DashboardStatsDto>()
               .ForMember(dest => dest.AttendanceDates, opt => opt.Ignore())
               .ForMember(dest => dest.WeightHistory, opt => opt.Ignore());

            CreateMap<UserDailyStats, WeightEntryDto>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
                .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Weight));

            CreateMap<AddDailyStatsDto, UserDailyStats>()
               .ForMember(dest => dest.UserId, opt => opt.Ignore())
               .ForMember(dest => dest.TotalCalories, opt => opt.Ignore())
               .ForMember(dest => dest.TotalSteps, opt => opt.Ignore())
               .ForMember(dest => dest.TotalWater, opt => opt.Ignore());


            CreateMap<AppUser, AIInputData>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

        }
    }
}
