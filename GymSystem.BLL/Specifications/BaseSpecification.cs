using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GymSystem.BLL.Specifications
{
    public abstract class BaseSpecification<T> : ISpecification<T>
    {
        public Expression<Func<T, bool>> Criteria { get; set; }
        public List<Expression<Func<T, object>>> Includes { get; set; } = new List<Expression<Func<T, object>>>();
        public Expression<Func<T, object>> OrderBy { get; set; }
        public Expression<Func<T, object>> OrderByDescending { get; set; }
        public int Take { get; set; }
        public int Skip { get; set; }
        public bool IsPagingEnabled { get; set; }

        protected BaseSpecification()
        {
        }

        protected BaseSpecification(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }

        public void AddIncludes(Expression<Func<T, object>> include)
        {
            Includes.Add(include);
        }

        public void AddOrderBy(Expression<Func<T, object>> orderBy)
        {
            OrderBy = orderBy;
        }

        public void AddOrderByDescending(Expression<Func<T, object>> orderByDesc)
        {
            OrderByDescending = orderByDesc;
        }

        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }

        protected void ApplySearchFilter<TProperty>(SpecPrams specParams, Expression<Func<T, TProperty>> propertySelector)
        {
            if (!string.IsNullOrEmpty(specParams.Search))
            {
                var searchValue = specParams.Search.ToLower();
                var parameter = propertySelector.Parameters[0];
                var property = propertySelector.Body;
                var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                var toLowerExpression = Expression.Call(property, toLowerMethod);
                var containsExpression = Expression.Call(toLowerExpression, containsMethod, Expression.Constant(searchValue));

                var lambda = Expression.Lambda<Func<T, bool>>(containsExpression, parameter);

                Criteria = Criteria == null
                    ? lambda
                    : Expression.Lambda<Func<T, bool>>(
                        Expression.AndAlso(Criteria.Body, lambda.Body),
                        Criteria.Parameters);
            }
        }

        protected void ApplyFilter<TProperty>(SpecPrams specParams, string propertyValue, Expression<Func<T, TProperty>> propertySelector)
        {
            if (!string.IsNullOrEmpty(propertyValue))
            {
                var parameter = propertySelector.Parameters[0];
                var property = propertySelector.Body;
                var equalsExpression = Expression.Equal(property, Expression.Constant(propertyValue));

                var lambda = Expression.Lambda<Func<T, bool>>(equalsExpression, parameter);

                Criteria = Criteria == null
                    ? lambda
                    : Expression.Lambda<Func<T, bool>>(
                        Expression.AndAlso(Criteria.Body, lambda.Body),
                        Criteria.Parameters);
            }
        }

        protected void ApplyPagination(SpecPrams specParams)
        {
            if (specParams.PageSize > 0)
            {
                ApplyPaging(specParams.PageSize * (specParams.PageIndex - 1), specParams.PageSize);
            }
        }
    }
}