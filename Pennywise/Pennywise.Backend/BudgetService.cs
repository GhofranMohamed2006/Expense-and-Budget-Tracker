using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Pennywise.Backend
{
    internal class BudgetService
    {
        private readonly Database _database = new Database();

        // 1. Add a new budget
        public bool AddBudget(Budget budget)
        {
            if (budget.BudgetAmount < 0)
            {
                throw new ArgumentException(
                    "Budget amount cannot be negative.");
            }

            string query = @"
                INSERT INTO Budgets
                    (UserID, Category, BudgetAmount, BudgetMonth)
                VALUES
                    (@UserID, @Category, @BudgetAmount, @BudgetMonth);";

            using (SqlConnection connection = _database.GetConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@UserID", budget.UserID);

                    command.Parameters.AddWithValue(
                        "@Category", budget.Category);

                    command.Parameters.AddWithValue(
                        "@BudgetAmount", budget.BudgetAmount);

                    command.Parameters.AddWithValue(
                        "@BudgetMonth",
                        new DateTime(
                            budget.BudgetMonth.Year,
                            budget.BudgetMonth.Month,
                            1));

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        // 2. Get all budgets for a user
        public List<Budget> GetBudgets(int userID)
        {
            List<Budget> budgets = new List<Budget>();

            string query = @"
                SELECT
                    BudgetID,
                    UserID,
                    Category,
                    BudgetAmount,
                    BudgetMonth
                FROM Budgets
                WHERE UserID = @UserID
                ORDER BY BudgetMonth DESC, Category;";

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
                            Budget budget = new Budget
                            {
                                BudgetID = reader.GetInt32(0),
                                UserID = reader.GetInt32(1),
                                Category = reader.GetString(2),
                                BudgetAmount = reader.GetDecimal(3),
                                BudgetMonth = reader.GetDateTime(4)
                            };

                            budgets.Add(budget);
                        }
                    }
                }
            }

            return budgets;
        }

        // 3. Delete a budget
        public bool DeleteBudget(int budgetID, int userID)
        {
            string query = @"
                DELETE FROM Budgets
                WHERE BudgetID = @BudgetID
                  AND UserID = @UserID;";

            using (SqlConnection connection = _database.GetConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BudgetID", budgetID);
                    command.Parameters.AddWithValue("@UserID", userID);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }
        public BudgetSummary GetBudgetSummary(int budgetID, int userID)
        {
            BudgetSummary summary = new BudgetSummary();
            string query = @"
            SELECT
                B.BudgetAmount,
                COALESCE(SUM(T.Amount), 0) AS SpentAmount
            FROM Budgets B
            LEFT JOIN Transactions T
                ON T.UserID = B.UserID
                AND T.Category = B.Category
                AND T.Type = 'Expense'
                AND T.TransactionDate >= B.BudgetMonth
                AND T.TransactionDate < DATEADD(MONTH, 1, B.BudgetMonth)
            WHERE B.BudgetID = @BudgetID
              AND B.UserID = @UserID
            GROUP BY B.BudgetAmount;";
            using (SqlConnection connection = _database.GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BudgetID", budgetID);
                    command.Parameters.AddWithValue("@UserID", userID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return summary;
                        }

                        summary.BudgetAmount = reader.GetDecimal(0);
                        summary.SpentAmount = reader.GetDecimal(1);

                        summary.RemainingAmount =
                            summary.BudgetAmount - summary.SpentAmount;

                        summary.UsagePercentage =
                            summary.BudgetAmount > 0
                                ? summary.SpentAmount / summary.BudgetAmount * 100
                                : 0;
                    }
                }
            }
            return summary;
        }
        public bool UpdateBudget(Budget budget)
        {
            if (budget.BudgetAmount < 0)
            {
                throw new ArgumentException(
                    "Budget amount cannot be negative.");
            }
            string query = @"
            UPDATE Budgets
            SET Category = @Category,
                BudgetAmount = @BudgetAmount,
                BudgetMonth = @BudgetMonth
            WHERE BudgetID = @BudgetID
              AND UserID = @UserID;";
            using (SqlConnection connection = _database.GetConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@BudgetID", budget.BudgetID);

                    command.Parameters.AddWithValue(
                        "@UserID", budget.UserID);

                    command.Parameters.AddWithValue(
                        "@Category", budget.Category);

                    command.Parameters.AddWithValue(
                        "@BudgetAmount", budget.BudgetAmount);

                    command.Parameters.AddWithValue(
                        "@BudgetMonth",
                        new DateTime(
                            budget.BudgetMonth.Year,
                            budget.BudgetMonth.Month,
                            1));

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
