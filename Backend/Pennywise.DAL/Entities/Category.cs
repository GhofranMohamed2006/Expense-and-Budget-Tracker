namespace Pennywise.DAL.Entities;

public class Category
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties (ERD: creates / limits / classifies)
    public User User { get; set; } = null!;
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
