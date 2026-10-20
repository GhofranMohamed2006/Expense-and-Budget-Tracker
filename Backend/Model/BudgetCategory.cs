namespace PennywiseApi.Models;

public class BudgetCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Limit { get; set; }
    public decimal Spent { get; set; }
}