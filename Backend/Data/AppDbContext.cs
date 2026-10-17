using Microsoft.EntityFrameworkCore;
using Pennywise.Api.Models;

namespace Pennywise.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserPreferences> Preferences => Set<UserPreferences>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        b.Entity<User>().HasOne(u => u.Preferences).WithOne().HasForeignKey<UserPreferences>(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<User>().HasMany(u => u.Categories).WithOne().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Category>().Property(c => c.Type).HasConversion<string>();
        b.Entity<Category>().HasIndex(c => new { c.UserId, c.Type, c.Name }).IsUnique();
        b.Entity<Category>().HasMany(c => c.Transactions).WithOne().HasForeignKey(t => t.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
