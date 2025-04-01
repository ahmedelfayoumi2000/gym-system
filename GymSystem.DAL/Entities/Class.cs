using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymSystem.DAL.Entities
{
    public class Class : BaseEntity
    {
        public string MemberName { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsDeleted { get; set; }

        public int? PlanId { get; set; }
        public Plan? Plan { get; set; }
        public string? TrainerId { get; set; }
        public AppUser? Trainer { get; set; }
    }
}