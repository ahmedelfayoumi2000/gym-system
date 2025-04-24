using Microsoft.EntityFrameworkCore;
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
        public List<(Expression<Func<T, object>> Collection, Expression<Func<object, object>> ThenInclude)> ThenIncludes { get; set; }
                = new List<(Expression<Func<T, object>>, Expression<Func<object, object>>)>();
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

        public void AddThenInclude<TCollection, TProperty>(
            Expression<Func<T, IEnumerable<TCollection>>> collectionSelector,
            Expression<Func<TCollection, TProperty>> thenIncludeSelector)
        {
            ThenIncludes.Add((
                collectionSelector as Expression<Func<T, object>>,
                thenIncludeSelector as Expression<Func<object, object>>
            ));
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

        protected void ApplySearchFilter(SpecPrams specParams, Expression<Func<T, string>> propertySelector)
        {
            if (!string.IsNullOrEmpty(specParams.Search))
            {
                var searchValue = $"%{specParams.Search.ToLower()}%";
                var parameter = propertySelector.Parameters[0];
                var property = propertySelector.Body;

                if (property.Type != typeof(string))
                {
                    throw new InvalidOperationException("The property selector must point to a string property.");
                }

                var likeExpression = Expression.Call(
                    typeof(DbFunctionsExtensions),
                    nameof(DbFunctionsExtensions.Like),
                    Type.EmptyTypes,
                    Expression.Constant(EF.Functions),
                    property,
                    Expression.Constant(searchValue));

                var lambda = Expression.Lambda<Func<T, bool>>(likeExpression, parameter);

                if (Criteria == null)
                {
                    Criteria = lambda;
                }
                else
                {
                    Criteria = Expression.Lambda<Func<T, bool>>(
                        Expression.AndAlso(Criteria.Body, lambda.Body),
                        Criteria.Parameters);
                }
            }
        }


        protected void ApplyFilter<TProperty>(SpecPrams specParams, TProperty propertyValue, Expression<Func<T, TProperty>> propertySelector)
        {
            if (propertyValue != null)
            {
                var parameter = propertySelector.Parameters[0];
                var property = propertySelector.Body;

                var convertedValue = Convert.ChangeType(propertyValue, typeof(TProperty));
                var equalsExpression = Expression.Equal(property, Expression.Constant(convertedValue, typeof(TProperty)));

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