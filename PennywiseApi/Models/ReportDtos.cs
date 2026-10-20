namespace PennywiseApi.Models;

/// <summary>Income, expenses and net for one period.</summary>
public record PeriodTotals(decimal Income, decimal Expenses, decimal Net);

/// <summary>Headline numbers for the Reports page.</summary>
/// <param name="SavingsRatePercent">Share of income that was kept, 0 to 100.</param>
/// <param name="Days">Length of the period in days.</param>
/// <param name="Previous">Totals for the period of the same length right before this one. Null unless both from and to are given.</param>
/// <param name="IncomeChangePercent">Change against the previous period. Null when there is no previous period or it had no income.</param>
public record OverviewDto(
    decimal Income,
    decimal Expenses,
    decimal Net,
    int SavingsRatePercent,
    int TransactionCount,
    int Days,
    decimal AverageDailyExpense,
    PeriodTotals? Previous,
    decimal? IncomeChangePercent,
    decimal? ExpenseChangePercent);

/// <summary>One calendar month. Month is "yyyy-MM".</summary>
public record MonthlyDto(string Month, decimal Income, decimal Expenses, decimal Net);

/// <summary>Total for one category with its share of all transactions of the chosen type.</summary>
public record CategoryTotalDto(string Category, decimal Total, int Count, decimal Percent);
