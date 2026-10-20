using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Pennywise.Backend
{
    internal class TransactionService
    {
        private readonly Database _database = new Database();
        public int AddTransaction(Transaction transaction)
        {
            string query = @"
        INSERT INTO Transactions
        (
            UserID,
            Category,
            Amount,
            Type,
            Description,
            TransactionDate
        )
        OUTPUT INSERTED.TransactionID
        VALUES
        (
            @UserID,
            @Category,
            @Amount,
            @Type,
            @Description,
            @TransactionDate
        )";

            using (SqlConnection connection = _database.GetConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", transaction.UserID);
                    command.Parameters.AddWithValue("@Category", transaction.Category);
                    command.Parameters.AddWithValue("@Amount", transaction.Amount);
                    command.Parameters.AddWithValue("@Type", transaction.Type);

                    command.Parameters.AddWithValue(
                        "@Description",
                        string.IsNullOrWhiteSpace(transaction.Description)
                            ? DBNull.Value
                            : transaction.Description);

                    command.Parameters.AddWithValue(
                        "@TransactionDate",
                        transaction.TransactionDate.Date);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public List<Transaction> GetAllTransactions(int userID)
        {
            List<Transaction> transactions = new List<Transaction>();

            string query = @"
        SELECT
            TransactionID,
            UserID,
            Category,
            Amount,
            Type,
            Description,
            TransactionDate
        FROM Transactions
        WHERE UserID = @UserID
        ORDER BY TransactionDate DESC, TransactionID DESC";
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
                            transactions.Add(new Transaction
                            {
                                TransactionID = reader.GetInt32(0),
                                UserID = reader.GetInt32(1),
                                Category = reader.GetString(2),
                                Amount = reader.GetDecimal(3),
                                Type = reader.GetString(4),
                                Description = reader.IsDBNull(5)
                                    ? string.Empty
                                    : reader.GetString(5),
                                TransactionDate = reader.GetDateTime(6)
                            });
                        }
                    }
                }
            }
            return transactions;
        }
        public List<Transaction> GetRecentTransactions(int userID)
        {
            List<Transaction> transactions = new List<Transaction>();
            string query = @" 
            SELECT TOP 5 
                TransactionID, UserID, Category, Amount, Type, Description, TransactionDate 
            FROM Transactions 
            WHERE UserID = @UserID 
            ORDER BY TransactionDate DESC, TransactionID DESC";
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
                            Transaction transaction = new Transaction
                            {
                                TransactionID = reader.GetInt32(0),
                                UserID = reader.GetInt32(1),
                                Category = reader.GetString(2),
                                Amount = reader.GetDecimal(3),
                                Type = reader.GetString(4),
                                Description = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                TransactionDate = reader.GetDateTime(6)
                            };
                            transactions.Add(transaction);
                        }
                    }
                }
            }
            return transactions;
        }
        public Transaction? GetTransactionById(int transactionID, int userID)
        {
            string query = @"
        SELECT
            TransactionID,
            UserID,
            Category,
            Amount,
            Type,
            Description,
            TransactionDate
        FROM Transactions
        WHERE TransactionID = @TransactionID
          AND UserID = @UserID";
            using (SqlConnection connection = _database.GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TransactionID", transactionID);
                    command.Parameters.AddWithValue("@UserID", userID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                            return null;

                        return new Transaction
                        {
                            TransactionID = reader.GetInt32(0),
                            UserID = reader.GetInt32(1),
                            Category = reader.GetString(2),
                            Amount = reader.GetDecimal(3),
                            Type = reader.GetString(4),
                            Description = reader.IsDBNull(5)
                                ? string.Empty
                                : reader.GetString(5),
                            TransactionDate = reader.GetDateTime(6)
                        };
                    }
                }
            }
        }
        public bool UpdateTransaction(Transaction transaction)
        {
            string query = @"
            UPDATE Transactions
            SET Category = @Category,
                Amount = @Amount,
                Type = @Type,
                Description = @Description,
                TransactionDate = @TransactionDate
            WHERE TransactionID = @TransactionID
              AND UserID = @UserID";
            using (SqlConnection connection = _database.GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TransactionID", transaction.TransactionID);
                    command.Parameters.AddWithValue("@UserID", transaction.UserID);
                    command.Parameters.AddWithValue("@Category", transaction.Category);
                    command.Parameters.AddWithValue("@Amount", transaction.Amount);
                    command.Parameters.AddWithValue("@Type", transaction.Type);
                    command.Parameters.AddWithValue(
                        "@Description",
                        string.IsNullOrWhiteSpace(transaction.Description)
                            ? DBNull.Value
                            : transaction.Description);
                    command.Parameters.AddWithValue(
                        "@TransactionDate",
                        transaction.TransactionDate.Date);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }
        public bool DeleteTransaction(int transactionID, int userID)
        {
            string query = @"
        DELETE FROM Transactions
        WHERE TransactionID = @TransactionID
          AND UserID = @UserID";

            using (SqlConnection connection = _database.GetConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TransactionID", transactionID);
                    command.Parameters.AddWithValue("@UserID", userID);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
