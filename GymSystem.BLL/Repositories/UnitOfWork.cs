using GymSystem.BLL.Interfaces;
using GymSystem.DAL.Data;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppIdentityDbContext _context;
        private Hashtable _repostories;

        public UnitOfWork(AppIdentityDbContext
            context)
        {
            _context = context;
        }

        //Save Changes
        public async Task<int> Complete()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            if (_repostories == null)
                _repostories = new Hashtable();
            var type = typeof(TEntity).Name;

            if (!_repostories.ContainsKey(type))
            {
                var repository = new GenericRepository<TEntity>(_context);
                _repostories.Add(type, repository);
            }
            return (IGenericRepository<TEntity>)_repostories[type];

        }
        public void DetachEntity<TEntity>(TEntity entity) where TEntity : class
        {
            var entry = _context.Entry(entity);
            if (entry != null)
            {
                entry.State = EntityState.Detached;
            }
        }
    }
}
