using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Pennywise.DAL.Context;

namespace Pennywise.DAL.Repositories;

public class GenericRepository<T>(AppDbContext context) : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext Context = context;
    protected DbSet<T> Set => Context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(int id) => await Set.FindAsync(id);
    public virtual Task<List<T>> GetAllAsync() => Set.AsNoTracking().ToListAsync();
    public virtual Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate) => Set.Where(predicate).ToListAsync();
    public virtual Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) => Set.AnyAsync(predicate);
    public virtual async Task AddAsync(T entity) => await Set.AddAsync(entity);
    public virtual void Update(T entity) => Set.Update(entity);
    public virtual void Remove(T entity) => Set.Remove(entity);
}
