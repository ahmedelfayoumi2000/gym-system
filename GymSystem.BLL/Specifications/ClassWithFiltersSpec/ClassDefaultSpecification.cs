using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities;

using System.Linq.Expressions;

/// <summary>
/// Default specification for retrieving classes with related data (Trainer, MonthlyMemberships, DailyAttendances, ClassEquipments).
/// </summary>
public class ClassDefaultSpecification : BaseSpecification<Class>
{
    public ClassDefaultSpecification()
    {
        AddIncludes(c => c.Trainer);
        //AddIncludes(c => c.MonthlyMemberships);
        //AddIncludes(c => c.DailyAttendances);
        AddIncludes(c => c.ClassEquipments);
    }

    public ClassDefaultSpecification(Expression<Func<Class, bool>> criteria) : base(criteria)
    {
        AddIncludes(c => c.Trainer);
        //AddIncludes(c => c.MonthlyMemberships);
        //AddIncludes(c => c.DailyAttendances);
        AddIncludes(c => c.ClassEquipments);
    }
}