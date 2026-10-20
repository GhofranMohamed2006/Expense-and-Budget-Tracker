using Microsoft.EntityFrameworkCore;
using Pennywise.Api.Models;
using PennywiseApi.Models;

namespace Pennywise.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserPreferences> Preferences => Set<UserPreferences>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();

    public DbSet<BudgetCategory> BudgetCategories => Set<BudgetCategory>();

    public DbSet<Goal> Goals => Set<Goal>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        b.Entity<User>().Property(u => u.Email).HasMaxLength(120);

        b.Entity<User>()
            .HasOne(u => u.Preferences)
            .WithOne()
            .HasForeignKey<UserPreferences>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<User>()
            .HasMany(u => u.Categories)
            .WithOne()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Category>().Property(c => c.Type).HasConversion<string>();
        b.Entity<Category>().HasAlternateKey(c => new { c.Id, c.UserId });

        b.Entity<Category>()
            .HasIndex(c => new { c.UserId, c.Type, c.Name })
            .IsUnique()
            .HasDatabaseName("IX_Categories_UserId_Type_Name");

        b.Entity<Category>()
            .HasMany(c => c.Transactions)
            .WithOne()
            .HasForeignKey(t => new { t.CategoryId, t.UserId })
            .HasPrincipalKey(c => new { c.Id, c.UserId })
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Transaction>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<Transaction>().Property(t => t.Amount).HasPrecision(18, 2);
        b.Entity<Transaction>().HasIndex(t => new { t.UserId, t.Date });

        b.Entity<RevokedToken>().HasIndex(token => token.TokenId).IsUnique();

        b.Entity<BudgetCategory>().ToTable("UserBudgets");
        b.Entity<BudgetCategory>().Property(budget => budget.Limit).HasPrecision(18, 2);
        b.Entity<BudgetCategory>()
            .HasIndex(budget => new { budget.UserId, budget.CategoryId })
            .IsUnique();
        b.Entity<BudgetCategory>()
            .HasOne<User>()
            .WithMany(u => u.Budgets)
            .HasForeignKey(budget => budget.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<BudgetCategory>()
            .HasOne<Category>()
            .WithMany(c => c.Budgets)
            .HasForeignKey(budget => new { budget.CategoryId, budget.UserId })
            .HasPrincipalKey(c => new { c.Id, c.UserId })
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Goal>().ToTable("UserGoals");
        b.Entity<Goal>().Property(goal => goal.Target).HasPrecision(18, 2);
        b.Entity<Goal>().Property(goal => goal.Saved).HasPrecision(18, 2);
        b.Entity<Goal>().HasIndex(goal => new { goal.UserId, goal.Name }).IsUnique();
        b.Entity<Goal>()
            .HasOne<User>()
            .WithMany(u => u.Goals)
            .HasForeignKey(goal => goal.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}