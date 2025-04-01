using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.Class
{
    public class ClassViewDto
    {
        public int Id { get; set; }
        public string MemberName { get; set; }
        public DateTime StartTime { get; set; }
        public Plan Plan { get; set; }
        public string? TrainerId { get; set; }

    }
}
