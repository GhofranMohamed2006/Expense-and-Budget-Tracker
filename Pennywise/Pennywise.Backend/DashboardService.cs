using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Pennywise.Backend
{
    internal class DashboardService
    {
        private readonly Database _database = new Database();
        public DashboardSummary GetSummary(int userID)
        {
            DashboardSummary summary = new DashboardSummary();
            string query = @" 
SELECT 
    COALESCE( 
        SUM(CASE 
            WHEN Type = 'Income' THEN Amount 
            ELSE 0 
        END), 0) AS TotalIncome, 
    COALESCE( 
        SUM(CASE 
            WHEN Type = 'Expense' THEN Amount 
            ELSE 0 
        END), 0) AS TotalExpenses 
FROM Transactions 
WHERE UserID = @UserID;";
            using (SqlConnection connection = _database.GetConnection()) 
            {
                connection.Open(); 
                using (SqlCommand command = new SqlCommand(query, connection)) 
                { 
                    command.Parameters.AddWithValue("@UserID", userID); 
                    using (SqlDataReader reader = command.ExecuteReader()) 
                    { 
                        if (reader.Read()) 
                        { 
                            summary.TotalIncome = reader.GetDecimal(0); 
                            summary.TotalExpenses = reader.GetDecimal(1); 
                            summary.TotalBalance = summary.TotalIncome - summary.TotalExpenses; 
                        } 
                    } 
                }
            }
            return summary;
        }
        public List<CategoryExpense> GetExpensesByCategory(int userID)
        {
            List<CategoryExpense> expenses = new List<CategoryExpense>();
        string query = @"
            SELECT
                Category,
                SUM(Amount) AS TotalExpenses
            FROM Transactions
            WHERE UserID = @UserID
              AND Type = 'Expense'
            GROUP BY Category
            ORDER BY TotalExpenses DESC";

        using (SqlConnection connection = _database.GetConnection())
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserID", userID);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CategoryExpense expense = new CategoryExpense
                        {
                            Category = reader.GetString(0),
                            TotalExpenses = reader.GetDecimal(1)
                        };
                        expenses.Add(expense);
                    }
                }
            }
        }
        return expenses;
        }
        public List<MonthlyExpense> GetMonthlyExpenses(int userID)
        {
            List<MonthlyExpense> expenses = new List<MonthlyExpense>();

            string query = @"
            SELECT
                YEAR(TransactionDate) AS ExpenseYear,
                MONTH(TransactionDate) AS ExpenseMonth,
                SUM(Amount) AS TotalExpenses
            FROM Transactions
            WHERE UserID = @UserID
              AND Type = 'Expense'
              AND TransactionDate >= DATEADD(
                  MONTH, -5,
                  DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)
              )
              AND TransactionDate < DATEADD(
                  MONTH, 1,
                  DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)
              )
            GROUP BY
                YEAR(TransactionDate),
                MONTH(TransactionDate)
            ORDER BY
                ExpenseYear,
                ExpenseMonth;";
            using (SqlConnection connection = _database.GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MonthlyExpense expense = new MonthlyExpense
                            {
                                Year = reader.GetInt32(0),
                                Month = reader.GetInt32(1),
                                TotalExpenses = reader.GetDecimal(2)
                            };
                            expenses.Add(expense);
                        }
                    }
                }
            }
            return expenses;
        }
    }
}
