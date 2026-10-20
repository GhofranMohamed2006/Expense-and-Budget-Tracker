namespace PennywiseApi.Models;

public class Goal
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = "";
    public decimal Target { get; set; }
    public decimal Saved { get; set; }
    public DateTime? Date { get; set; }
}