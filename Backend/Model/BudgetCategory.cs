using System.ComponentModel.DataAnnotations.Schema;

namespace PennywiseApi.Models;

public class BudgetCategory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = "";
    public decimal Limit { get; set; }

    [NotMapped]
    public decimal Spent { get; set; }
}