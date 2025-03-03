using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Data;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Identity;
using Microsoft.EntityFrameworkCore;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly AppIdentityDbContext _context;

    public GenericRepository(AppIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
        => await _context.Set<T>().ToListAsync();
    public async Task<T> GetByIdAsync(int id)
      => await _context.Set<T>().FindAsync(id);

    public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> spec)
        => await ApplySpecifications(spec).ToListAsync();
        
        
    public async Task<T> GetByIdWithSpecAsync(ISpecification<T> spec)
      => await ApplySpecifications(spec).FirstOrDefaultAsync();
      

    private IQueryable<T> ApplySpecifications(ISpecification<T> spec)
        =>  SpecificationsEvaiuator<T>.GetQuery(_context.Set<T>(), spec);
         

    public async Task<int> GetCountAsync(ISpecification<T> spec)
       => await ApplySpecifications(spec).CountAsync();

    public async Task Add(T entity)
      => await _context.Set<T>().AddAsync(entity);

    public void Update(T entity)
        => _context.Set<T>().Update(entity);
   
    public void Delete(T entity)
      => _context.Set<T>().Remove(entity);

    public async Task<T> GetEntityWithSpecAsync(ISpecification<T> spec)
    {
        return await ApplySpecifications(spec).FirstOrDefaultAsync();
    }
}