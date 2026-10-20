using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pennywise.Backend
{
    internal class ApiServer
    {
        private readonly HttpListener _listener = new HttpListener();

        private readonly DashboardService _dashboardService =
            new DashboardService();

        private readonly TransactionService _transactionService =
            new TransactionService();

        private readonly BudgetService _budgetService =
            new BudgetService();

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task StartAsync()
        {
            _listener.Prefixes.Add("http://localhost:5050/");
            _listener.Start();

            Console.WriteLine("Pennywise API is running!");
            Console.WriteLine("Address: http://localhost:5050/");
            Console.WriteLine("Press Ctrl + C to stop.");

            while (_listener.IsListening)
            {
                HttpListenerContext context =
                    await _listener.GetContextAsync();

                _ = Task.Run(() => HandleRequestAsync(context));
            }
        }

        private async Task HandleRequestAsync(
            HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;

            try
            {
                response.Headers["Access-Control-Allow-Origin"] = "*";

                response.Headers["Access-Control-Allow-Methods"] =
                    "GET, POST, PUT, DELETE, OPTIONS";

                response.Headers["Access-Control-Allow-Headers"] =
                    "Content-Type";

                if (context.Request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 204;
                    return;
                }

                string path = context.Request.Url!.AbsolutePath;
                string method = context.Request.HttpMethod;

                // =====================================
                // DASHBOARD SUMMARY
                // GET /api/dashboard/summary?userId=1
                // =====================================
                if (path == "/api/dashboard/summary" && method == "GET")
                {
                    if (!TryGetUserId(context, out int userID))
                    {
                        await SendJsonAsync(response,
                            new { message = "Invalid User ID." }, 400);
                        return;
                    }

                    var summary = _dashboardService.GetSummary(userID);

                    await SendJsonAsync(response, summary, 200);
                }

                // =====================================
                // BUDGETS
                // GET /api/budgets?userId=1
                // =====================================
                else if (path == "/api/budgets" && method == "GET")
                {
                    if (!TryGetUserId(context, out int userID))
                    {
                        await SendJsonAsync(response,
                            new { message = "Invalid User ID." }, 400);
                        return;
                    }

                    var budgets = _budgetService.GetBudgets(userID);

                    var result = budgets.Select(budget =>
                    {
                        var summary = _budgetService.GetBudgetSummary(
                            budget.BudgetID, userID);

                        return new
                        {
                            budget.BudgetID,
                            budget.UserID,
                            budget.Category,
                            budget.BudgetAmount,
                            budget.BudgetMonth,
                            summary.SpentAmount,
                            summary.RemainingAmount,
                            summary.UsagePercentage
                        };
                    }).ToList();

                    await SendJsonAsync(response, result, 200);
                }

                // =====================================
                // RECENT TRANSACTIONS
                // GET /api/transactions/recent?userId=1
                // =====================================
                else if (path == "/api/transactions/recent"
                    && method == "GET")
                {
                    if (!TryGetUserId(context, out int userID))
                    {
                        await SendJsonAsync(response,
                            new { message = "Invalid User ID." }, 400);
                        return;
                    }

                    var transactions =
                        _transactionService.GetRecentTransactions(userID);

                    await SendJsonAsync(response, transactions, 200);
                }

                // =====================================
                // GET ALL TRANSACTIONS
                // GET /api/transactions?userId=1
                // =====================================
                else if (path == "/api/transactions" && method == "GET")
                {
                    if (!TryGetUserId(context, out int userID))
                    {
                        await SendJsonAsync(response,
                            new { message = "Invalid User ID." }, 400);
                        return;
                    }

                    var transactions =
                        _transactionService.GetAllTransactions(userID);

                    await SendJsonAsync(response, transactions, 200);
                }

                // =====================================
                // ADD TRANSACTION
                // POST /api/transactions
                // =====================================
                else if (path == "/api/transactions" && method == "POST")
                {
                    using var reader = new StreamReader(
                        context.Request.InputStream,
                        context.Request.ContentEncoding);

                    string body = await reader.ReadToEndAsync();

                    Transaction? transaction;

                    try
                    {
                        transaction = JsonSerializer.Deserialize<Transaction>(
                            body, JsonOptions);
                    }
                    catch (JsonException)
                    {
                        await SendJsonAsync(response,
                            new { message = "Invalid JSON." }, 400);
                        return;
                    }

                    if (transaction == null ||
                        transaction.Amount <= 0 ||
                        string.IsNullOrWhiteSpace(transaction.Category) ||
                        transaction.Category.Length > 100 ||
                        !IsValidType(transaction.Type) ||
                        transaction.Description?.Length > 255 ||
                        transaction.TransactionDate == default)
                    {
                        await SendJsonAsync(response,
                            new { message = "Invalid transaction data." }, 400);
                        return;
                    }

                    // Temporary user ID for the current project.
                    // Replace with authenticated user ID when login is implemented.
                    transaction.UserID = 1;

                    int newID =
                        _transactionService.AddTransaction(transaction);

                    await SendJsonAsync(response, new
                    {
                        message = "Transaction added successfully!",
                        transactionID = newID
                    }, 201);
                }

                // =====================================
                // GET / UPDATE / DELETE BY ID
                // =====================================
                else if (path.StartsWith("/api/transactions/"))
                {
                    string idPart = path.Substring(
                        "/api/transactions/".Length);

                    if (!int.TryParse(idPart, out int transactionID) ||
                        transactionID <= 0)
                    {
                        await SendJsonAsync(response,
                            new { message = "Invalid transaction ID." }, 400);
                        return;
                    }

                    if (!TryGetUserId(context, out int userID))
                    {
                        await SendJsonAsync(response,
                            new { message = "Invalid User ID." }, 400);
                        return;
                    }

                    // GET TRANSACTION BY ID
                    if (method == "GET")
                    {
                        var transaction =
                            _transactionService.GetTransactionById(
                                transactionID, userID);

                        if (transaction == null)
                        {
                            await SendJsonAsync(response,
                                new { message = "Transaction not found." }, 404);
                            return;
                        }

                        await SendJsonAsync(response, transaction, 200);
                    }

                    // UPDATE TRANSACTION
                    else if (method == "PUT")
                    {
                        using var reader = new StreamReader(
                            context.Request.InputStream,
                            context.Request.ContentEncoding);

                        string body = await reader.ReadToEndAsync();

                        Transaction? transaction;

                        try
                        {
                            transaction = JsonSerializer
                                .Deserialize<Transaction>(body, JsonOptions);
                        }
                        catch (JsonException)
                        {
                            await SendJsonAsync(response,
                                new { message = "Invalid JSON." }, 400);
                            return;
                        }

                        if (transaction == null ||
                            transaction.Amount <= 0 ||
                            string.IsNullOrWhiteSpace(transaction.Category) ||
                            transaction.Category.Length > 100 ||
                            !IsValidType(transaction.Type) ||
                            transaction.Description?.Length > 255 ||
                            transaction.TransactionDate == default)
                        {
                            await SendJsonAsync(response,
                                new { message = "Invalid transaction data." }, 400);
                            return;
                        }

                        transaction.TransactionID = transactionID;
                        transaction.UserID = userID;

                        bool updated =
                            _transactionService.UpdateTransaction(transaction);

                        if (!updated)
                        {
                            await SendJsonAsync(response,
                                new { message = "Transaction not found." }, 404);
                            return;
                        }

                        await SendJsonAsync(response,
                            new { message = "Transaction updated successfully!" },
                            200);
                    }

                    // DELETE TRANSACTION
                    else if (method == "DELETE")
                    {
                        bool deleted =
                            _transactionService.DeleteTransaction(
                                transactionID, userID);

                        if (!deleted)
                        {
                            await SendJsonAsync(response,
                                new { message = "Transaction not found." }, 404);
                            return;
                        }

                        await SendJsonAsync(response,
                            new { message = "Transaction deleted successfully!" },
                            200);
                    }

                    else
                    {
                        await SendJsonAsync(response,
                            new { message = "Method not allowed." }, 405);
                    }
                }

                // =====================================
                // CATEGORY EXPENSES
                // =====================================
                else if (path == "/api/dashboard/category-expenses"
                    && method == "GET")
                {
                    if (!TryGetUserId(context, out int userID))
                    {
                        await SendJsonAsync(response,
                            new { message = "Invalid User ID." }, 400);
                        return;
                    }

                    var expenses =
                        _dashboardService.GetExpensesByCategory(userID);

                    await SendJsonAsync(response, expenses, 200);
                }

                // =====================================
                // MONTHLY EXPENSES
                // =====================================
                else if (path == "/api/dashboard/monthly-expenses"
                    && method == "GET")
                {
                    if (!TryGetUserId(context, out int userID))
                    {
                        await SendJsonAsync(response,
                            new { message = "Invalid User ID." }, 400);
                        return;
                    }

                    var expenses =
                        _dashboardService.GetMonthlyExpenses(userID);

                    await SendJsonAsync(response, expenses, 200);
                }
                // POST /api/budgets
                else if (path == "/api/budgets" && method == "POST")
                {
                    Budget? budget = await JsonSerializer.DeserializeAsync<Budget>(
                        context.Request.InputStream,
                        JsonOptions
                    );

                    if (budget == null ||
                        string.IsNullOrWhiteSpace(budget.Category) ||
                        budget.Category.Length > 100 ||
                        budget.BudgetAmount <= 0 ||
                        budget.BudgetMonth == default)
                    {
                        await SendJsonAsync(
                            context.Response,
                            new { message = "Invalid budget data." },
                            400
                        );
                        return;
                    }

                    budget.UserID = 1;

                    bool added = _budgetService.AddBudget(budget);

                    if (added)
                    {
                        await SendJsonAsync(
                            context.Response,
                            new { message = "Budget added successfully." },
                            201
                        );
                    }
                    else
                    {
                        await SendJsonAsync(
                            context.Response,
                            new { message = "Could not add budget." },
                            400
                        );
                    }
                }

                // PUT /api/budgets/{id}
                else if (path.StartsWith("/api/budgets/") && method == "PUT")
                {
                    string idText = path.Substring("/api/budgets/".Length);

                    if (!int.TryParse(idText, out int budgetID) || budgetID <= 0)
                    {
                        await SendJsonAsync(
                            context.Response,
                            new { message = "Invalid budget ID." },
                            400
                        );
                        return;
                    }

                    Budget? budget = await JsonSerializer.DeserializeAsync<Budget>(
                        context.Request.InputStream,
                        JsonOptions
                    );

                    if (budget == null ||
                        string.IsNullOrWhiteSpace(budget.Category) ||
                        budget.Category.Length > 100 ||
                        budget.BudgetAmount <= 0 ||
                        budget.BudgetMonth == default)
                    {
                        await SendJsonAsync(
                            context.Response,
                            new { message = "Invalid budget data." },
                            400
                        );
                        return;
                    }

                    budget.BudgetID = budgetID;
                    budget.UserID = 1;

                    bool updated = _budgetService.UpdateBudget(budget);

                    if (updated)
                    {
                        await SendJsonAsync(
                            context.Response,
                            new { message = "Budget updated successfully." },
                            200
                        );
                    }
                    else
                    {
                        await SendJsonAsync(
                            context.Response,
                            new { message = "Budget not found or could not be updated." },
                            404
                        );
                    }
                }

                // DELETE /api/budgets/{id}?userId=1
                else if (path.StartsWith("/api/budgets/") && method == "DELETE")
                {
                    string idText = path.Substring("/api/budgets/".Length);

                    if (!int.TryParse(idText, out int budgetID) || budgetID <= 0)
                    {
                        await SendJsonAsync(
                            context.Response,
                            new { message = "Invalid budget ID." },
                            400
                        );
                        return;
                    }

                    if (!TryGetUserId(context, out int userID))
                    {
                        await SendJsonAsync(
                            context.Response,
                            new { message = "Invalid user ID." },
                            400
                        );
                        return;
                    }

                    bool deleted = _budgetService.DeleteBudget(budgetID, userID);

                    if (deleted)
                    {
                        await SendJsonAsync(
                            context.Response,
                            new { message = "Budget deleted successfully." },
                            200
                        );
                    }
                    else
                    {
                        await SendJsonAsync(
                            context.Response,
                            new { message = "Budget not found." },
                            404
                        );
                    }
                }
                // =====================================
                // UNKNOWN ENDPOINT
                // =====================================
                else
                {
                    await SendJsonAsync(response,
                        new { message = "Endpoint not found." }, 404);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error: {ex}");

                if (response.OutputStream.CanWrite)
                {
                    try
                    {
                        await SendJsonAsync(response,
                            new { message = "Internal server error." }, 500);
                    }
                    catch
                    {
                        // Response may already be closed.
                    }
                }
            }
            finally
            {
                response.Close();
            }
        }

        // =====================================
        // VALIDATE TRANSACTION TYPE
        // =====================================
        private static bool IsValidType(string? type)
        {
            return string.Equals(type, "Income",
                       StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, "Expense",
                       StringComparison.OrdinalIgnoreCase);
        }

        // =====================================
        // READ USER ID
        // =====================================
        private static bool TryGetUserId(
            HttpListenerContext context,
            out int userID)
        {
            string? value = context.Request.QueryString["userId"];

            return int.TryParse(value, out userID) && userID > 0;
        }

        // =====================================
        // SEND JSON RESPONSE
        // =====================================
        private static async Task SendJsonAsync(
            HttpListenerResponse response,
            object data,
            int statusCode)
        {
            response.StatusCode = statusCode;
            response.ContentType = "application/json; charset=utf-8";
            response.ContentEncoding = Encoding.UTF8;

            string json = JsonSerializer.Serialize(data);

            byte[] buffer = Encoding.UTF8.GetBytes(json);

            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}