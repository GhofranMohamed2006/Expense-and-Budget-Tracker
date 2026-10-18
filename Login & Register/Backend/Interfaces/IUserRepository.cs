using PennywiseAuth.Models;

namespace PennywiseAuth.Interfaces;

/// <summary>
/// Defines the data access contract required by the Pennywise Authentication module.
/// 
/// ARCHITECTURE NOTE FOR SHARED DATABASE INTEGRATION:
/// This interface decouples authentication business logic from underlying storage technology.
/// The default implementation is <see cref="Repositories.SqliteUserRepository"/>, intended strictly
/// as an isolated development adapter.
/// 
/// To connect this module to the team's shared Pennywise database:
/// 1. Implement this interface in your shared infrastructure project (e.g., PostgreSQL, SQL Server, EF Core).
/// 2. Map existing user tables to the <see cref="AuthUser"/> model or adapt your schema accordingly.
/// 3. Inject your production repository instance in <see cref="Program.cs"/> where IUserRepository is registered.
/// 4. Do not modify the authentication service or controller logic.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Initializes schema/connection if required (e.g. creating development tables).
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Finds a user by their normalized email address.
    /// Returns null if no user matches.
    /// </summary>
    Task<AuthUser?> GetByEmailAsync(string email);

    /// <summary>
    /// Checks if a user already exists with the given email address.
    /// </summary>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Persists a new user record with their hashed credentials.
    /// Returns the persisted AuthUser containing the generated identifier.
    /// </summary>
    Task<AuthUser> CreateUserAsync(AuthUser user);

    /// <summary>
    /// Updates the last login timestamp for an existing user.
    /// </summary>
    Task UpdateLastLoginAsync(int userId, DateTime lastLogin);
}
