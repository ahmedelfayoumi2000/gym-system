using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities.Identity;
using GymSystem.DAL.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories.Business
{
    public class IdentityUserRepository : IIdentityUserRepository
    {
        private readonly AppIdentityDbContext _context;

        public IdentityUserRepository(AppIdentityDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IReadOnlyList<AppUser>> GetAllWithSpecAsync(ISpecification<AppUser> spec)
        {
            try
            {
                var query = _context.Users.AsQueryable();
                query = ApplySpecification(query, spec);
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve users with specification", ex);
            }
        }

        public async Task<int> GetCountAsync(ISpecification<AppUser> spec)
        {
            try
            {
                var query = _context.Users.AsQueryable();
                query = ApplySpecification(query, spec);
                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to count users with specification", ex);
            }
        }

        public async Task<AppUser> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(id));
            }

            try
            {
                return await _context.Users.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve user with ID: {id}", ex);
            }
        }

        private IQueryable<AppUser> ApplySpecification(IQueryable<AppUser> query, ISpecification<AppUser> spec)
        {
            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

            if (spec.Includes.Any())
            {
                query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (spec.OrderBy != null)
            {
                query = query.OrderBy(spec.OrderBy);
            }
            else if (spec.OrderByDescending != null)
            {
                query = query.OrderByDescending(spec.OrderByDescending);
            }

            if (spec.IsPagingEnabled)
            {
                query = query.Skip(spec.Skip).Take(spec.Take);
            }

            return query;
        }
    }
}