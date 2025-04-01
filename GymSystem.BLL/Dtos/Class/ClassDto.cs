using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.Class
{
    public class ClassDto
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "You must Enter a Name")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "MemberName must be between 1 and 100 characters.")]
        public string MemberName { get; set; }

        public DateTime? StartTime { get; set; }

        [Required(ErrorMessage = "PlanId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "PlanId must be a positive integer.")]
        public int PlanId { get; set; }

        public string? TrainerId { get; set; }
    }
}
