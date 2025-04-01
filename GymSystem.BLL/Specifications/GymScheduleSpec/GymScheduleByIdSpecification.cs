using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.GymScheduleSpec
{
    public class GymScheduleByIdSpecification : BaseSpecification<GymSchedule>
    {
        public GymScheduleByIdSpecification(int scheduleId)
            : base(s => s.Id == scheduleId && s.IsActive)
        {
        }
    }
}