namespace Pennywise.DAL.Entities;

public class Budget
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties (ERD: sets / limits)
    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
