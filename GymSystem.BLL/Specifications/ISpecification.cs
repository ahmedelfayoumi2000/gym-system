using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GymSystem.BLL.Specifications
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> Criteria { get; }
        List<Expression<Func<T, object>>> Includes { get; set; }
        List<(Expression<Func<T, object>> Collection, Expression<Func<object, object>> ThenInclude)> ThenIncludes { get; set; }
        Expression<Func<T, object>> OrderBy { get; set; }
        Expression<Func<T, object>> OrderByDescending { get; set; }
        int Take { get; set; }
        int Skip { get; set; }
        bool IsPagingEnabled { get; set; }
    }
}