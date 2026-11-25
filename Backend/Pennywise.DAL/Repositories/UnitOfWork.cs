using Pennywise.DAL.Context;
using Pennywise.DAL.Entities;

namespace Pennywise.DAL.Repositories;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ICategoryRepository Categories { get; }
    IGenericRepository<Account> Accounts { get; }
    IGenericRepository<Budget> Budgets { get; }
    IGenericRepository<Transaction> Transactions { get; }
    Task<int> SaveChangesAsync();
}

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IUserRepository? _users;
    private ICategoryRepository? _categories;
    private IGenericRepository<Account>? _accounts;
    private IGenericRepository<Budget>? _budgets;
    private IGenericRepository<Transaction>? _transactions;

    public IUserRepository Users => _users ??= new UserRepository(context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(context);
    public IGenericRepository<Account> Accounts => _accounts ??= new GenericRepository<Account>(context);
    public IGenericRepository<Budget> Budgets => _budgets ??= new GenericRepository<Budget>(context);
    public IGenericRepository<Transaction> Transactions => _transactions ??= new GenericRepository<Transaction>(context);

    public Task<int> SaveChangesAsync() => context.SaveChangesAsync();
}
