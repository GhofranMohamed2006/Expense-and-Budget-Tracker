using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pennywise.DAL.Entities;

namespace Pennywise.DAL.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users");
        b.HasKey(x => x.Id);
        b.Property(x => x.FullName).IsRequired().HasMaxLength(100);
        b.Property(x => x.Email).IsRequired().HasMaxLength(150);
        b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(200);
        b.HasIndex(x => x.Email).IsUnique();
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("Categories");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(50);
        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
        b.HasIndex(x => new { x.UserId, x.Type, x.Name }).IsUnique();

        // Users 1 --- M Categories  (creates)
        b.HasOne(x => x.User).WithMany(u => u.Categories)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> b)
    {
        b.ToTable("Budgets");
        b.HasKey(x => x.Id);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.HasIndex(x => new { x.UserId, x.CategoryId, x.Month, x.Year }).IsUnique();

        // Users 1 --- M Budgets  (sets)
        b.HasOne(x => x.User).WithMany(u => u.Budgets)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        // Categories 1 --- M Budgets  (limits)
        b.HasOne(x => x.Category).WithMany(c => c.Budgets)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> b)
    {
        b.ToTable("Accounts");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(80);
        b.Property(x => x.AccountType).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.OpeningBalance).HasPrecision(18, 2);
        b.HasIndex(x => new { x.UserId, x.Name }).IsUnique();

        // Users 1 --- M Accounts  (owns)
        b.HasOne(x => x.User).WithMany(u => u.Accounts)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> b)
    {
        b.ToTable("Transactions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Description).HasMaxLength(250);
        b.HasIndex(x => new { x.UserId, x.TransactionDate });

        // Users 1 --- M Transactions  (records)
        b.HasOne(x => x.User).WithMany(u => u.Transactions)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        // Categories 1 --- M Transactions  (classifies)
        b.HasOne(x => x.Category).WithMany(c => c.Transactions)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        // Accounts 1 --- M Transactions  (contains)
        b.HasOne(x => x.Account).WithMany(a => a.Transactions)
            .HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
        // Transfer destination account
        b.HasOne(x => x.TransferToAccount).WithMany(a => a.IncomingTransfers)
            .HasForeignKey(x => x.TransferToAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}
