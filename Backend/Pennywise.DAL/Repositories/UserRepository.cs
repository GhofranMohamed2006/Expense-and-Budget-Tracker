using Microsoft.EntityFrameworkCore;
using Pennywise.DAL.Context;
using Pennywise.DAL.Entities;

namespace Pennywise.DAL.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
}

public class UserRepository(AppDbContext context) : GenericRepository<User>(context), IUserRepository
{
    public Task<User?> GetByEmailAsync(string email) => Set.FirstOrDefaultAsync(u => u.Email == email);
    public Task<bool> EmailExistsAsync(string email) => Set.AnyAsync(u => u.Email == email);
}
