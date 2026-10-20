using System.Data;
using System.Globalization;
using Microsoft.Data.Sqlite;
using PennywiseAuth.Interfaces;
using PennywiseAuth.Models;

namespace PennywiseAuth.Repositories;

/// <summary>
/// Development-only SQLite adapter for the Pennywise Authentication module.
/// 
/// DISCLAIMER:
/// This adapter is strictly an isolated development mechanism allowing local validation
/// and demonstration of genuine registration and login flows. It is NOT the shared
/// production database for Pennywise.
/// 
/// HOW TO REPLACE WITH THE PRODUCTION DATABASE:
/// 1. Create a class implementing <see cref="IUserRepository"/> in your data project.
/// 2. Connect to the shared database (e.g. PostgreSQL, SQL Server, MySQL, or main Pennywise DB).
/// 3. Map to your production users table.
/// 4. Update the dependency registration in Program.cs to instantiate your production repository.
/// </summary>
public class SqliteUserRepository : IUserRepository
{
    private readonly string _connectionString;
    private const string TableName = "pennywise_auth_dev_users";

    public SqliteUserRepository(string connectionString = "Data Source=pennywise_dev.db")
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Initializes the isolated development table if it does not already exist.
    /// Does not touch any other application tables.
    /// </summary>
    public async Task InitializeAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        string ddl = $@"
            CREATE TABLE IF NOT EXISTS {TableName} (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                email TEXT NOT NULL UNIQUE COLLATE NOCASE,
                password_hash TEXT NOT NULL,
                salt TEXT NOT NULL,
                created_at TEXT NOT NULL,
                last_login_at TEXT
            );
            CREATE INDEX IF NOT EXISTS idx_{TableName}_email ON {TableName}(email);
        ";

        using var command = new SqliteCommand(ddl, connection);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Retrieves a user by normalized email using a parameterized SQL query.
    /// </summary>
    public async Task<AuthUser?> GetByEmailAsync(string email)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        string query = $@"
            SELECT id, name, email, password_hash, salt, created_at, last_login_at
            FROM {TableName}
            WHERE email = @Email
            LIMIT 1;
        ";

        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@Email", email.Trim().ToLowerInvariant());

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new AuthUser
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                Salt = reader.GetString(4),
                CreatedAt = DateTime.Parse(reader.GetString(5), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                LastLoginAt = reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            };
        }

        return null;
    }

    /// <summary>
    /// Checks if an email is already registered using a parameterized SQL query.
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        string query = $"SELECT COUNT(1) FROM {TableName} WHERE email = @Email;";
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@Email", email.Trim().ToLowerInvariant());

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt64(result) > 0;
    }

    /// <summary>
    /// Persists a new user record using parameterized SQL.
    /// </summary>
    public async Task<AuthUser> CreateUserAsync(AuthUser user)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        string query = $@"
            INSERT INTO {TableName} (name, email, password_hash, salt, created_at, last_login_at)
            VALUES (@Name, @Email, @PasswordHash, @Salt, @CreatedAt, @LastLoginAt);
            SELECT last_insert_rowid();
        ";

        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@Name", user.Name.Trim());
        command.Parameters.AddWithValue("@Email", user.Email.Trim().ToLowerInvariant());
        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        command.Parameters.AddWithValue("@Salt", user.Salt);
        command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt.ToString("o"));
        command.Parameters.AddWithValue("@LastLoginAt", (object?)user.LastLoginAt?.ToString("o") ?? DBNull.Value);

        var idResult = await command.ExecuteScalarAsync();
        user.Id = Convert.ToInt32(idResult);
        return user;
    }

    /// <summary>
    /// Updates the last login timestamp for the given user ID.
    /// </summary>
    public async Task UpdateLastLoginAsync(int userId, DateTime lastLogin)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        string query = $"UPDATE {TableName} SET last_login_at = @LastLogin WHERE id = @Id;";
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@LastLogin", lastLogin.ToString("o"));
        command.Parameters.AddWithValue("@Id", userId);

        await command.ExecuteNonQueryAsync();
    }
}
