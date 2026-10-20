using Microsoft.EntityFrameworkCore;
using PennywiseApi.Models;

namespace PennywiseApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<BudgetCategory> BudgetCategories => Set<BudgetCategory>();
    public DbSet<Goal> Goals => Set<Goal>();
}