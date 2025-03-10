using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos
{
    public class ClassDto
    {
        public int? id { get; set; }

        [Required(ErrorMessage = " You must Enter A Name")]
        public string MemberName { get; set; }
        public DateTime? StartTime { get; set; }

        [Required(ErrorMessage = "PlanId is required.")]
        public int PlanId { get; set; }
        public string? TrainerId { get; set; }

    }
}
