using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pennywise.DAL.Context;
using Pennywise.DAL.Repositories;

namespace Pennywise.DAL;

public static class DalServiceExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
