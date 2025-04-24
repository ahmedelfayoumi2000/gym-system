using GymSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications
{
    public class SpecificationsEvaiuator<TEntity> where TEntity : BaseEntity
    {
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> spec)
        {
            var query = inputQuery;
            if (spec.Criteria != null)
                query = query.Where(spec.Criteria);

            if (spec.OrderBy != null)
                query = query.OrderBy(spec.OrderBy);

            if (spec.OrderByDescending != null)
                query = query.OrderByDescending(spec.OrderByDescending);
            if (spec.IsPagingEnabled)
                query = query.Skip(spec.Skip).Take(spec.Take);

            //context.Set<Product>()

            query = spec.Includes.Aggregate(query, (currentQuery, include) => currentQuery.Include(include));

            if (typeof(TEntity) == typeof(WorkoutPlan))
            {
                foreach (var (collection, thenInclude) in spec.ThenIncludes)
                {
                    // تحويل الـ Query لـ WorkoutPlan
                    var workoutPlanQuery = query as IQueryable<WorkoutPlan>;
                    if (workoutPlanQuery != null)
                    {
                        query = workoutPlanQuery
                            .Include(w => w.Exercises)
                            .ThenInclude(e => e.ExerciseCategory) as IQueryable<TEntity>;
                    }
                }
            }
            else if (typeof(TEntity) == typeof(UserFavoriteExercise))
            {
                foreach (var (collection, thenInclude) in spec.ThenIncludes)
                {
                    var userFavoriteExerciseQuery = query as IQueryable<UserFavoriteExercise>;
                    if (userFavoriteExerciseQuery != null)
                    {
                        query = userFavoriteExerciseQuery
                            .Include(u => u.Exercises) // افتراض إن العلاقة اسمها Exercise
                            .ThenInclude(e => e.ExerciseCategory) as IQueryable<TEntity>;
                    }
                }
            }

            return query;
        }

    }
}
