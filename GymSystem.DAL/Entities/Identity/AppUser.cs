using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace GymSystem.DAL.Entities.Identity
{
    public class AppUser : IdentityUser
    {
        public string DisplayName { get; set; } 
        public Address? Address { get; set; }
        public int UserRole { get; set; } // مثل Admin=1, Trainer=2, Receptionist=3, Member=4
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? ProfileImageName { get; set; } 
        public string? UserCode { get; set; }
        public string? Gender { get; set; }
        public uint? Age { get; set; }
        public decimal? Salary { get; set; }
        public DateTime StartDate { get; set; }= DateTime.Now;

		private DateTime _endDate;
		public DateTime EndDate
		{
			get => _endDate;
			set => _endDate = value == default ? DateTime.Now : value;
		}

		public bool IsStopped { get; set; } = false; 
        public DateTime? StopDate { get; set; }
        //public int? HaveDays { get; set; }
        public string? AddBy { get; set; } 
        public int? RemainingDays
		{
			get
			{
				TimeSpan remainingTime = EndDate - DateTime.Now;
				return remainingTime.Days;
			}
		}

        public bool IsProfileConfirmed { get; set; }
        public string Goal { get; set; } // "WeightLoss", "WeightGain", "MuscleGain", "BetterBodyShape"
        public string FitnessLevel { get; set; } // "Beginner", "Intermediate", "Advanced"
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

        public ICollection<DailyAttendance> DailyAttendances { get; set; } = new List<DailyAttendance>();
        public ICollection<MonthlyMembership> MonthlyMemberships { get; set; } = new List<MonthlyMembership>();

        public int? NutritionPlanId { get; set; }
        public NutritionPlan nutritionPlan { get; set; }
        public Membership membership { get; set; }
        public ICollection<WorkoutPlan> WorkoutPlans { get; set; }
        public ICollection<BMIRecord> BMIRecords { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; }

        public ICollection<Class> Classes { get; set; } = new List<Class>(); 
        public ICollection<EquipmentMaintenance> MaintainedEquipments { get; set; } = new List<EquipmentMaintenance>();
        
    }
}
