using Microsoft.EntityFrameworkCore;
using PennywiseApi.Models;

namespace PennywiseApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(e =>
        {
            e.Property(t => t.Description).HasMaxLength(120).IsRequired();
            e.Property(t => t.Category).HasMaxLength(40).IsRequired();
            e.Property(t => t.Account).HasMaxLength(40).IsRequired();
            e.Property(t => t.Type).HasConversion<string>().HasMaxLength(10);
            e.HasIndex(t => t.Date);
        });
    }
}

public static class SeedData
{
    /// <summary>Same example rows the page shows, so both start identical.</summary>
    public static void Run(AppDbContext db)
    {
        if (db.Transactions.Any()) return;

        Transaction T(string desc, string cat, TransactionType type, decimal amt, string date, string acct) => new()
        {
            Description = desc, Category = cat, Type = type, Amount = amt,
            Date = DateOnly.Parse(date), Account = acct
        };

        db.Transactions.AddRange(
            T("Monthly salary", "Salary", TransactionType.Income, 18500m, "2026-10-01", "Main account"),
            T("Logo design project", "Freelance", TransactionType.Income, 3200m, "2026-10-09", "Main account"),
            T("Seoudi Market", "Groceries", TransactionType.Expense, 842.50m, "2026-10-13", "Main account"),
            T("Uber to Cairo", "Transport", TransactionType.Expense, 215m, "2026-10-12", "Cash wallet"),
            T("Electricity bill", "Bills", TransactionType.Expense, 468m, "2026-10-11", "Main account"),
            T("Cilantro Cafe", "Dining", TransactionType.Expense, 186m, "2026-10-11", "Cash wallet"),
            T("Pharmacy", "Health", TransactionType.Expense, 129.75m, "2026-10-10", "Cash wallet"),
            T("Internet, WE", "Bills", TransactionType.Expense, 310m, "2026-10-08", "Main account"),
            T("Spotify", "Subscriptions", TransactionType.Expense, 59m, "2026-10-07", "Main account"),
            T("Carrefour", "Groceries", TransactionType.Expense, 1126.30m, "2026-10-06", "Main account"),
            T("Sneakers", "Shopping", TransactionType.Expense, 1450m, "2026-10-05", "Main account"),
            T("Metro card top-up", "Transport", TransactionType.Expense, 100m, "2026-10-04", "Cash wallet"),
            T("Koshary Abou Tarek", "Dining", TransactionType.Expense, 74m, "2026-10-03", "Cash wallet"),
            T("Savings transfer", "Salary", TransactionType.Income, 1500m, "2026-10-02", "Savings"));

        db.SaveChanges();
    }
}
